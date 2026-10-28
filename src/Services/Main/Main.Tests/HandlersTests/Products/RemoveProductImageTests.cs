using System.Text.Json;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Handlers.Products;
using Main.Application.Interfaces.Products;
using Main.Application.Lrts.DeleteProductImages;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Products;

public class RemoveProductImageTests : IntegrationTest
{
	public RemoveProductImageTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
	}

	private int ProductId => GetContext<ProductTestContext>().Products[0].Id;
	private S3StorageServiceStub S3 => Sp.GetRequiredService<S3StorageServiceStub>();

	[Fact]
	public async Task RemoveImage_QueuesCleanupAndInvalidatesCache_ThenLrtDeletesOnlyItsObject()
	{
		var image = ProductImage.CreateFromStorageKey(ProductId, "first.jpg");
		var otherImage = ProductImage.CreateFromStorageKey(ProductId, "second.jpg");
		await Context.AddRangeAsync([image, otherImage], CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		Scope.ServiceProvider.GetRequiredService<IDomainEventScope>().Flush();
		S3.SetFile("images", image.StorageKey, "image");
		S3.SetFile("images", otherImage.StorageKey, "preserved image");
		var provider = Scope.ServiceProvider.GetRequiredService<IProductProvider>();
		(await provider.GetProductOrSetAsync(ProductId, CancellationToken)).Images.Should().HaveCount(2);

		await Mediator.Send(new RemoveProductImageCommand(ProductId, $"https://images.example.com/{image.StorageKey}"),
			CancellationToken);

		(await Context.ProductImages.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which.StorageKey.Should().Be(otherImage.StorageKey);
		(await provider.GetProductAsync(ProductId, CancellationToken)).Should().BeNull();
		(await S3.DownloadFileAsync("images", image.StorageKey, CancellationToken)).IsSuccess.Should().BeTrue();
		var job = await Context.Set<Job>().SingleAsync(item => item.SystemName == DeleteProductImagesLrt.Name, CancellationToken);
		var input = JsonSerializer.Deserialize<DeleteProductImagesInputState>(job.State)!;
		input.StorageKeys.Should().Equal(image.StorageKey);
		input.ProductId.Should().Be(ProductId);
		input.BucketName.Should().Be("images");

		var leaseHolder = Guid.NewGuid();
		job.AcquireLease(leaseHolder, TimeSpan.FromMinutes(5));
		await Context.SaveChangesAsync(CancellationToken);
		var lrt = ActivatorUtilities.CreateInstance<DeleteProductImagesLrt>(Scope.ServiceProvider);
		await lrt.ExecuteAsync(job.Id, leaseHolder, CancellationToken);

		(await Context.Set<Job>().AsNoTracking().SingleAsync(item => item.Id == job.Id, CancellationToken))
			.Status.Should().Be(JobStatus.Succeeded);
		(await S3.DownloadFileAsync("images", image.StorageKey, CancellationToken)).IsSuccess.Should().BeFalse();
		(await S3.DownloadFileAsync("images", otherImage.StorageKey, CancellationToken)).IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task OuterTransactionRollback_PreservesImageAndObjectWithoutCleanupJob()
	{
		var image = ProductImage.Create(ProductId, ".jpg");
		await Context.AddAsync(image, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		Scope.ServiceProvider.GetRequiredService<IDomainEventScope>().Flush();
		S3.SetFile("images", image.StorageKey, "image");
		var transaction = Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();

		var action = () => transaction.WithSettings(TransactionalAttribute.ReadCommitted(0, 0)).ExecuteAsync(async (_, ct) =>
		{
			await Mediator.Send(new RemoveProductImageCommand(ProductId, image.StorageKey), ct);
			throw new InvalidOperationException("Rollback test");
		}, CancellationToken);

		await action.Should().ThrowAsync<InvalidOperationException>();

		(await Context.ProductImages.AsNoTracking().AnyAsync(item => item.ProductId == ProductId, CancellationToken))
			.Should().BeTrue();
		(await Context.Set<Job>().AsNoTracking().AnyAsync(item => item.SystemName == DeleteProductImagesLrt.Name, CancellationToken))
			.Should().BeFalse();
		(await S3.DownloadFileAsync("images", image.StorageKey, CancellationToken)).IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task MissingImage_ThrowsNotFoundWithoutQueuingCleanup()
	{
		var action = () => Mediator.Send(new RemoveProductImageCommand(ProductId, "missing.jpg"), CancellationToken);

		await action.Should().ThrowAsync<ProductImageNotFoundException>();
		(await Context.Set<Job>().AsNoTracking().AnyAsync(item => item.SystemName == DeleteProductImagesLrt.Name, CancellationToken))
			.Should().BeFalse();
	}
}
