using Application.Common.Interfaces.Cache;
using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Handlers.Products;
using Main.Application.Interfaces.Products;
using Main.Application.Lrts.ProductImageImport;
using Main.Application.Static;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProductImageImportLrtTests : CsvLrtIntegrationTest<ProductImageImportLrt>
{
	public ProductImageImportLrtTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<ProductTestContext>();

	private ProductTestContext TestContext => GetContext<ProductTestContext>();

	[Fact]
	public async Task ImportsExistingKeys_WithoutDuplicatesOrRemovingOtherImages()
	{
		var products = TestContext.Products.Take(2).ToArray();
		var existingImage = ProductImage.Create(products[0].Id, ".png");
		Context.ProductImages.Add(existingImage);
		await Context.SaveChangesAsync(CancellationToken);
		string[] rows =
		[
			CsvRow(products[0].Id, "Photo.JPG"),
			CsvRow(products[0].Id, "Photo.JPG"),
			CsvRow(products[1].Id, "Other.JPG"),
			CsvRow(products[0].Id, "supplier/Фото товара.webp")
		];

		for (var attempt = 0; attempt < 2; attempt++)
		{
			var execution = await ExecuteCsv("ProductId,FileName", rows,
				fileName => new ProductImageImportInputState { FileName = fileName });

			execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
			var state = execution.GetState<ProductImageImportState>();
			state.Checkpoint!.CurrentLine.Should().Be(4);
			state.Errors.Should().BeEmpty();
		}

		var keys = (await Context.ProductImages.AsNoTracking().ToListAsync(CancellationToken))
			.Select(image => image.GetId());
		keys.Should().BeEquivalentTo(new[]
		{
			existingImage.GetId(),
			(products[0].Id, $"products/{products[0].Id}/Photo.JPG"),
			(products[1].Id, $"products/{products[1].Id}/Other.JPG"),
			(products[0].Id, $"products/{products[0].Id}/supplier/Фото товара.webp")
		});
	}

	[Fact]
	public async Task InvalidRows_AreReportedWithoutDiscardingValidImages()
	{
		var productId = TestContext.Products[0].Id;
		var execution = await ExecuteCsv(
			"ProductId,FileName",
			[
				CsvRow(0, "photo.jpg"),
				CsvRow(-1, "photo.jpg"),
				CsvRow("not-an-id", "photo.jpg"),
				CsvRow(productId, ""),
				CsvRow(productId, "products/"),
				CsvRow(productId, "photo.exe"),
				CsvRow(productId, "../photo.jpg"),
				CsvRow(productId, "https://example.com/photo.jpg"),
				CsvRow(productId, new string('a', 244) + ".jpg"),
				CsvRow(productId, "photo.webp")
			],
			fileName => new ProductImageImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductImageImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(10);
		state.Errors.Select(error => error.RowIdx).Should().Equal(Enumerable.Range(1, 9));
		var image = (await Context.ProductImages.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		image.GetId().Should().Be((productId, $"products/{productId}/photo.webp"));
	}

	[Fact]
	public async Task MissingProduct_FailsTheJobAndRollsBackTheWholeBatch()
	{
		var productId = TestContext.Products[0].Id;
		var execution = await ExecuteCsv(
			"ProductId,FileName",
			[
				CsvRow(productId, "first.jpg"),
				CsvRow(int.MaxValue, "missing.jpg"),
				CsvRow(productId, "last.jpg")
			],
			fileName => new ProductImageImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Failed);
		execution.Job.Attempts.Should().Be(1);
		execution.Job.ErrorMessage.Should().Contain("product_images_product_id_fk");
		execution.GetState<ProductImageImportState>().Checkpoint.Should().BeNull();
		(await Context.ProductImages.AsNoTracking().ToListAsync(CancellationToken)).Should().BeEmpty();
	}

	[Fact]
	public async Task Import_InvalidatesCachedProductImages()
	{
		var productId = TestContext.Products[0].Id;
		await Scope.ServiceProvider.GetRequiredService<ICache>()
			.RemoveKeyAsync(CacheKeys.ProductCache.Product(productId));
		var provider = Scope.ServiceProvider.GetRequiredService<IProductProvider>();
		var cached = await provider.GetProductOrSetAsync(productId, CancellationToken);
		cached.Images.Should().BeEmpty();

		var execution = await ExecuteCsv("ProductId,FileName", [CsvRow(productId, "Photo.JPG")],
			fileName => new ProductImageImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var updated = await provider.GetProductOrSetAsync(productId, CancellationToken);
		updated.Images.Should().ContainSingle().Which.Should().Be($"https://images.example.com/products/{productId}/Photo.JPG");
	}

	[Fact]
	public async Task RemoveImage_DeletesItsObjectAndPreservesOtherImages()
	{
		var productId = TestContext.Products[0].Id;
		var storageKey = $"products/{productId}/first.jpg";
		var otherStorageKey = $"products/{productId}/second.jpg";
		var storage = Scope.ServiceProvider.GetRequiredService<S3StorageServiceStub>();
		storage.SetFile("images", storageKey, "uploaded image");
		storage.SetFile("images", otherStorageKey, "another image");
		var execution = await ExecuteCsv(
			"ProductId,FileName",
			[CsvRow(productId, "first.jpg"), CsvRow(productId, "second.jpg")],
			fileName => new ProductImageImportInputState { FileName = fileName });
		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);

		await Mediator.Send(new RemoveProductImageCommand(productId, $"https://images.example.com/{storageKey}"),
			CancellationToken);

		var remaining = (await Context.ProductImages.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		remaining.GetId().Should().Be((productId, otherStorageKey));
		using var preserved = await storage.DownloadFileAsync("images", otherStorageKey, CancellationToken);
		preserved.IsSuccess.Should().BeTrue();
		using var deleted = await storage.DownloadFileAsync("images", storageKey, CancellationToken);
		deleted.IsSuccess.Should().BeFalse();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task SameFileNameForDifferentProducts_UsesSeparateObjects(bool imageAlreadyAssigned)
	{
		var products = TestContext.Products.Take(3).ToArray();
		if (imageAlreadyAssigned)
		{
			Context.ProductImages.Add(ProductImage.CreateFromStorageKey(products[0].Id, "owned.jpg"));
			await Context.SaveChangesAsync(CancellationToken);
		}

		var rows = new List<string>();
		if (!imageAlreadyAssigned)
			rows.Add(CsvRow(products[0].Id, "owned.jpg"));
		rows.Add(CsvRow(products[2].Id, "fresh.jpg"));
		rows.Add(CsvRow(products[1].Id, "owned.jpg"));

		var execution = await ExecuteCsv("ProductId,FileName", rows,
			fileName => new ProductImageImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		execution.Job.Attempts.Should().Be(1);
		var state = execution.GetState<ProductImageImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(rows.Count);
		state.Errors.Should().BeEmpty();
		var images = await Context.ProductImages.AsNoTracking().ToListAsync(CancellationToken);
		images.Select(image => image.GetId()).Should().BeEquivalentTo(new[]
		{
			(products[0].Id, $"products/{products[0].Id}/owned.jpg"),
			(products[1].Id, $"products/{products[1].Id}/owned.jpg"),
			(products[2].Id, $"products/{products[2].Id}/fresh.jpg")
		});
	}

	[Fact]
	public async Task DuplicateAcrossBatches_IsIgnoredAndCheckpointAdvances()
	{
		var productId = TestContext.Products[0].Id;
		var execution = await ExecuteCsv(
			"ProductId,FileName",
			Enumerable.Range(1, 1001).Select(_ => CsvRow(productId, "photo.jpg")),
			fileName => new ProductImageImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductImageImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(1001);
		state.Errors.Should().BeEmpty();
		(await Context.ProductImages.AsNoTracking().ToListAsync(CancellationToken)).Should().ContainSingle();
	}
}
