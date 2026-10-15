using Contracts.Products;
using FluentAssertions;
using Main.Application.Interfaces.Products;
using Main.Entities.Product;
using Main.Enums;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Characteristics;

public class ProductEventSelectionTests : IntegrationTest
{
	public ProductEventSelectionTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
	}

	[Fact]
	public async Task SelectedPart_ProducesEventEvenWhenItsListIsEmpty()
	{
		var provider = Scope.ServiceProvider.GetRequiredService<IProductEventProvider>();

		var events = await provider.GetEventsAsync([int.MaxValue], ProductEventParts.Characteristics, CancellationToken);

		var notification = events.Should().ContainSingle().Which.Should()
			.BeOfType<ProductCharacteristicsUpdatedEvent>().Which;
		notification.Products.Should().BeEmpty();
		notification.UpdatedAt.Kind.Should().Be(DateTimeKind.Utc);
	}

	[Fact]
	public async Task UnselectedPart_DoesNotProduceEventEvenWhenItsListIsPopulated()
	{
		var productId = GetContext<ProductTestContext>().Products[0].Id;
		var characteristic = await new CharacteristicBuilder(Faker).WithName("Temperature").BuildAndAddToDb(Context);
		await Context.ProductCharacteristics.AddAsync(ProductCharacteristic.Create(productId, characteristic.Id, "100"), CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		var provider = Scope.ServiceProvider.GetRequiredService<IProductEventProvider>();

		var events = await provider.GetEventsAsync([productId], ProductEventParts.Core, CancellationToken);

		events.Should().ContainSingle().Which.Should().BeOfType<ProductsUpdatedEvent>();
	}

	[Fact]
	public async Task CombinedFlags_ProduceOnlyTheSelectedEvents()
	{
		var productId = GetContext<ProductTestContext>().Products[0].Id;
		var provider = Scope.ServiceProvider.GetRequiredService<IProductEventProvider>();

		var events = await provider.GetEventsAsync([productId],
			ProductEventParts.Characteristics | ProductEventParts.Weight, CancellationToken);

		events.Should().HaveCount(2);
		events.OfType<ProductCharacteristicsUpdatedEvent>().Should().ContainSingle();
		events.OfType<ProductWeightUpdatedEvent>().Should().ContainSingle();
	}

	[Fact]
	public async Task NoSelectedParts_ProducesNoEvents()
	{
		var productId = GetContext<ProductTestContext>().Products[0].Id;
		var provider = Scope.ServiceProvider.GetRequiredService<IProductEventProvider>();

		var events = await provider.GetEventsAsync([productId], (ProductEventParts)0, CancellationToken);

		events.Should().BeEmpty();
	}

	[Fact]
	public async Task NoProductIds_ProducesNoEvents()
	{
		var provider = Scope.ServiceProvider.GetRequiredService<IProductEventProvider>();

		var events = await provider.GetEventsAsync([], ProductEventParts.All, CancellationToken);

		events.Should().BeEmpty();
	}

	[Fact]
	public async Task MissingProduct_OnlyAddsDeletionToCoreEvent()
	{
		var productId = GetContext<ProductTestContext>().Products[0].Id;
		var provider = Scope.ServiceProvider.GetRequiredService<IProductEventProvider>();

		var events = await provider.GetEventsAsync([productId, int.MaxValue], ProductEventParts.All, CancellationToken);

		events.Should().HaveCount(4);
		var products = events.OfType<ProductsUpdatedEvent>().Should().ContainSingle().Which.Products;
		products.Should().HaveCount(2);
		products.Should().ContainSingle(product => product.Id == productId && !product.IsDeleted);
		products.Should().ContainSingle(product => product.Id == int.MaxValue && product.IsDeleted);
		events.OfType<ProductCharacteristicsUpdatedEvent>().Should().ContainSingle().Which.Products.Should()
			.ContainSingle().Which.ProductId.Should().Be(productId);
		events.OfType<ProductDimensionsUpdatedEvent>().Should().ContainSingle().Which.Products.Should()
			.ContainSingle().Which.ProductId.Should().Be(productId);
		events.OfType<ProductWeightUpdatedEvent>().Should().ContainSingle().Which.Products.Should()
			.ContainSingle().Which.ProductId.Should().Be(productId);
	}
}
