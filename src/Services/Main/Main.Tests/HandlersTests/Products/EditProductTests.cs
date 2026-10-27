using Abstractions.Models;
using Application.Common.Models;
using Application.Common.Interfaces.Events;
using FluentAssertions;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.PatchProduct;
using Main.Application.Interfaces.Products;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Products;

public class EditProductTests : IntegrationTest
{
	public EditProductTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
	}

	private ProductTestContext TestContext => GetContext<ProductTestContext>();

	[Fact]
	public async Task NameOnly_PersistsChangeAndInvalidatesProductCache()
	{
		var (reloaded, before) = await PatchAndReload(
			new PatchProductDto { Name = PatchField<string>.From("Updated product name") },
			updated => updated.Name.Value.Should().Be("Updated product name"));

		reloaded.Should().BeEquivalentTo(before with { Name = "Updated product name" });
	}

	[Fact]
	public async Task SkuOnly_PersistsChangeAndInvalidatesProductCache()
	{
		var (reloaded, before) = await PatchAndReload(
			new PatchProductDto { Sku = PatchField<string>.From("SINGLE-FIELD-SKU") },
			updated => updated.Sku.Value.Should().Be("SINGLE-FIELD-SKU"));

		reloaded.Should().BeEquivalentTo(before with { Sku = "SINGLE-FIELD-SKU" });
	}

	[Fact]
	public async Task DescriptionOnly_PersistsChangeAndInvalidatesProductCache()
	{
		var (reloaded, before) = await PatchAndReload(
			new PatchProductDto { Description = PatchField<string?>.From("Updated description") },
			updated => updated.Description.Should().Be("Updated description"));

		reloaded.Should().BeEquivalentTo(before with { Description = "Updated description" });
	}

	[Fact]
	public async Task IndicatorOnly_PersistsChangeAndInvalidatesProductCache()
	{
		var (reloaded, before) = await PatchAndReload(
			new PatchProductDto { Indicator = PatchField<string?>.From("Updated indicator") },
			updated => updated.Indicator!.Value.Should().Be("Updated indicator"));

		reloaded.Should().BeEquivalentTo(before with { Indicator = "Updated indicator" });
	}

	[Fact]
	public async Task ProducerOnly_PersistsChangeAndInvalidatesProductCache()
	{
		var product = TestContext.Products[0];
		var producer = TestContext.ProducerTestContext.Producers.First(item => item.Id != product.ProducerId);
		var (reloaded, before) = await PatchAndReload(
			new PatchProductDto { ProducerId = PatchField<int>.From(producer.Id) },
			updated => updated.ProducerId.Should().Be(producer.Id));

		reloaded.Should().BeEquivalentTo(before with { ProducerId = producer.Id, ProducerName = producer.Name });
	}

	[Fact]
	public async Task GroupOnly_PersistsChangeAndInvalidatesProductCache()
	{
		var group = await new ProductGroupBuilder(Faker).BuildAndAddToDb(Context);
		var (reloaded, before) = await PatchAndReload(
			new PatchProductDto { GroupId = PatchField<int?>.From(group.Id) },
			updated => updated.ProductGroupId.Should().Be(group.Id));

		reloaded.Should().BeEquivalentTo(before with { GroupId = group.Id });
	}

	[Fact]
	public async Task PackingUnitOnly_PersistsChangeAndInvalidatesProductCache()
	{
		var (reloaded, before) = await PatchAndReload(
			new PatchProductDto { PackingUnit = PatchField<int?>.From(12) },
			updated => updated.PackingUnit.Should().Be(12));

		reloaded.Should().BeEquivalentTo(before);
	}

	[Fact]
	public async Task PairOnly_PersistsChangeAndInvalidatesProductCache()
	{
		var pairId = TestContext.Products[1].Id;
		var (reloaded, before) = await PatchAndReload(
			new PatchProductDto { PairId = PatchField<int?>.From(pairId) },
			updated => updated.PairId.Should().Be(pairId));

		reloaded.Should().BeEquivalentTo(before);
	}

	[Fact]
	public async Task DescriptionOnly_CanBeClearedAndInvalidatesProductCache()
	{
		TestContext.Products[0].SetDescription("Original description");
		await Context.SaveChangesAsync(CancellationToken);
		var (reloaded, before) = await PatchAndReload(
			new PatchProductDto { Description = PatchField<string?>.From(null) },
			updated => updated.Description.Should().BeNull());

		reloaded.Should().BeEquivalentTo(before with { Description = null });
	}

	[Fact]
	public async Task IndicatorOnly_CanBeClearedAndInvalidatesProductCache()
	{
		TestContext.Products[0].SetIndicator("Original indicator");
		await Context.SaveChangesAsync(CancellationToken);
		var (reloaded, before) = await PatchAndReload(
			new PatchProductDto { Indicator = PatchField<string?>.From(null) },
			updated => ((string?)updated.Indicator).Should().BeNull());

		reloaded.Should().BeEquivalentTo(before with { Indicator = null });
	}

	[Fact]
	public async Task InvalidName_DoesNotChangeDatabaseOrInvalidateProductCache()
	{
		var id = GetFirstId();
		var provider = Scope.ServiceProvider.GetRequiredService<IProductProvider>();
		var before = await provider.GetProductOrSetAsync(id, CancellationToken);

		var action = () => Mediator.Send(new PatchProductCommand(id,
			new PatchProductDto { Name = PatchField<string>.From("ab") }), CancellationToken);

		await action.Should().ThrowAsync<ValidationException>();
		var updated = await GetProduct(id);
		updated!.Name.Value.Should().Be(before.Name);
		(await provider.GetProductAsync(id, CancellationToken)).Should().BeEquivalentTo(before);
	}

	[Fact]
	public async Task EditArticle_NumberAndName_Succeeds()
	{
		var id = GetFirstId();
		var command = new PatchProductCommand(
			id,
			new PatchProductDto
			{
				Sku = new PatchField<string>
				{
					IsSet = true, Value = "67890"
				},
				Name = new PatchField<string>
				{
					IsSet = true, Value = "Updated Article"
				}
			});

		var act = () => Mediator.Send(command);

		await act.Should().NotThrowAsync();

		var updatedProduct = await GetProduct(id);
		updatedProduct.Should().NotBeNull();

		updatedProduct.Name.Value.Should().Be("Updated Article");
		updatedProduct.Sku.Value.Should().Be("67890");
	}

	[Fact]
	public async Task EditArticle_WithInvalidArticleId_FailsValidation()
	{
		var command = new PatchProductCommand(
			999,
			new PatchProductDto
			{
				Sku = new PatchField<string>
				{
					IsSet = true, Value = "67890"
				}
			});

		var act = () => Mediator.Send(command);

		await act.Should().ThrowAsync<ProductNotFoundException>();
	}

	[Fact]
	public async Task EditArticle_WithEmptyArticleNumber_FailsValidation()
	{
		var command = new PatchProductCommand(
			GetFirstId(),
			new PatchProductDto
			{
				Sku = new PatchField<string>
				{
					IsSet = true, Value = ""
				}
			});

		var act = () => Mediator.Send(command);
		await act.Should().ThrowAsync<ValidationException>();
	}

	[Fact]
	public async Task EditArticle_WhenNothingEdited_Succeeds()
	{
		var command = new PatchProductCommand(
			GetFirstId(),
			new PatchProductDto
			{
				Sku = new PatchField<string>
				{
					IsSet = false, Value = null
				}
			});

		var act = () => Mediator.Send(command);
		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task EditArticle_WhenArticleNumberNull_FailsValidation()
	{
		var command = new PatchProductCommand(
			GetFirstId(),
			new PatchProductDto
			{
				Sku = new PatchField<string>
				{
					IsSet = true, Value = null
				}
			});
		await Assert.ThrowsAsync<ValidationException>(async () =>
			await Mediator.Send(command, CancellationToken));
	}

	private async Task<(ProductDto Reloaded, ProductDto Before)> PatchAndReload(
		PatchProductDto patch, Action<Product> assertDatabase)
	{
		var id = GetFirstId();
		Scope.ServiceProvider.GetRequiredService<IDomainEventScope>().Flush();
		var provider = Scope.ServiceProvider.GetRequiredService<IProductProvider>();
		var before = await provider.GetProductOrSetAsync(id, CancellationToken);
		(await provider.GetProductAsync(id, CancellationToken)).Should().NotBeNull("the cache must be populated before the update");
		var otherId = TestContext.Products[1].Id;
		var otherCached = await provider.GetProductOrSetAsync(otherId, CancellationToken);

		await Mediator.Send(new PatchProductCommand(id, patch), CancellationToken);

		var updated = await Context.Products.AsNoTracking().SingleAsync(product => product.Id == id, CancellationToken);
		assertDatabase(updated);
		(await provider.GetProductAsync(id, CancellationToken)).Should().BeNull("the handler must invalidate the cached product");
		(await provider.GetProductAsync(otherId, CancellationToken)).Should().BeEquivalentTo(otherCached);
		var reloaded = await provider.GetProductOrSetAsync(id, CancellationToken);
		return (reloaded, before);
	}

	private int GetFirstId() => TestContext.Products[0].Id;

	private async Task<Product?> GetProduct(int productId) =>
		await Context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == productId, CancellationToken);
}
