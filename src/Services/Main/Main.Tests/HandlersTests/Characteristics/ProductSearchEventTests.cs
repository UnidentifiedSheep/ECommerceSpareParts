using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Products;
using Contracts.Storage;
using Domain.Events;
using Enums.Units;
using FluentAssertions;
using Main.Application.Products.Interfaces;
using Main.Application.Products.Services;
using Main.Entities.DomainEvents.Product;
using Main.Entities.Product;
using Main.Enums;
using Main.Modules.Currency.Persistence;
using Main.Modules.Product.Persistence;
using Main.Modules.Storage.Persistence;
using Main.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.Persistence.Interceptors;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Characteristics;

public class ProductSearchEventTests : IntegrationTest
{
	public ProductSearchEventTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<ProductTestContext>();

	[Fact]
	public async Task CaptureAll_ReturnsProductAndEmptyNullableSectionsWithoutChangingProduct()
	{
		var product = GetContext<ProductTestContext>().Products[0];
		var provider = Scope.ServiceProvider.GetRequiredService<IProductEventProvider>();
		var version = product.RowVersion;
		var events = await provider.GetEventsAsync(
			[product.Id, product.Id],
			ProductEventParts.All,
			CancellationToken);

		events.Should().HaveCount(5);
		var core = GetEvent<ProductsUpdatedEvent>(events);
		core.UpdatedAt.Kind.Should().Be(DateTimeKind.Utc);
		core.Products.Should().ContainSingle().Which.Sku.Should().Be(product.Sku.Value);
		var characteristics = GetEvent<ProductCharacteristicsUpdatedEvent>(events);
		characteristics.Products.Should().ContainSingle().Which.Characteristics.Should().BeEmpty();
		characteristics.UpdatedAt.Should().Be(core.UpdatedAt);
		var dimensions = GetEvent<ProductDimensionsUpdatedEvent>(events);
		dimensions.Products.Should().ContainSingle().Which.Dimensions.Should().BeNull();
		dimensions.UpdatedAt.Should().Be(core.UpdatedAt);
		var weight = GetEvent<ProductWeightUpdatedEvent>(events);
		weight.Products.Should().ContainSingle().Which.Weight.Should().BeNull();
		weight.UpdatedAt.Should().Be(core.UpdatedAt);
		var stock = GetEvent<ProductStocksSnapshotEvent>(events);
		stock.OccurredAt.Should().Be(core.UpdatedAt);
		stock.Products.Should().ContainSingle().Which.Stocks.Should().BeEmpty();
		product.RowVersion.Should().Be(version);
	}

	[Fact]
	public async Task DeletingLastCharacteristic_ProducesAnEmptySnapshot()
	{
		var productId = GetContext<ProductTestContext>().Products[0].Id;
		var characteristic =
			await new CharacteristicBuilder(Faker).WithName("Temperature").BuildAndAddToDb(Context);
		var link = ProductCharacteristic.Create(
			productId,
			characteristic.Id,
			"100");
		await Context.Set<ProductCharacteristic>().AddAsync(link, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		var provider = Scope.ServiceProvider.GetRequiredService<IProductEventProvider>();
		var initial = GetEvent<ProductCharacteristicsUpdatedEvent>(
			await provider.GetEventsAsync(
				[productId],
				ProductEventParts.Characteristics,
				CancellationToken));
		initial.Products
			.Should()
			.ContainSingle()
			.Which
			.Characteristics
			.Should()
			.ContainSingle()
			.Which
			.Value
			.Should()
			.Be("100");

		Context.Set<ProductCharacteristic>().Remove(link);
		await Context.SaveChangesAsync(CancellationToken);
		var notification = GetEvent<ProductCharacteristicsUpdatedEvent>(
			await provider.GetEventsAsync(
				[productId],
				ProductEventParts.Characteristics,
				CancellationToken));

		notification.Products.Should().ContainSingle().Which.Characteristics.Should().BeEmpty();
		notification.UpdatedAt.Should().BeAfter(initial.UpdatedAt);
	}

	[Fact]
	public async Task LifecycleBatches_PublishCurrentCharacteristicsSnapshot()
	{
		var productId = GetContext<ProductTestContext>().Products[0].Id;
		var characteristic =
			await new CharacteristicBuilder(Faker).WithName("Temperature").BuildAndAddToDb(Context);
		var link = ProductCharacteristic.Create(
			productId,
			characteristic.Id,
			"100");
		await Context.Set<ProductCharacteristic>().AddAsync(link, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		var scope = Scope.ServiceProvider.GetRequiredService<IIntegrationEventScope>();
		scope.Flush();

		await Mediator.Publish(
			new Batch<EntityCreatedDomainEvent<ProductCharacteristic>>(
			[
				new EntityCreatedDomainEvent<ProductCharacteristic>(link),
				new EntityCreatedDomainEvent<ProductCharacteristic>(link)
			]),
			CancellationToken);
		var created = scope.Flush()
			.Should()
			.ContainSingle()
			.Which
			.Message
			.Should()
			.BeOfType<ProductCharacteristicsUpdatedEvent>()
			.Which;
		created.Products
			.Should()
			.ContainSingle()
			.Which
			.Characteristics
			.Should()
			.ContainSingle()
			.Which
			.Value
			.Should()
			.Be("100");

		link.SetValue("200");
		await Context.SaveChangesAsync(CancellationToken);
		await Mediator.Publish(
			new Batch<EntityUpdatedDomainEvent<ProductCharacteristic, (int, int)>>(
				[new EntityUpdatedDomainEvent<ProductCharacteristic, (int, int)>(link.GetId())]),
			CancellationToken);
		var updated = scope.Flush()
			.Should()
			.ContainSingle()
			.Which
			.Message
			.Should()
			.BeOfType<ProductCharacteristicsUpdatedEvent>()
			.Which;
		updated.Products
			.Should()
			.ContainSingle()
			.Which
			.Characteristics
			.Should()
			.ContainSingle()
			.Which
			.Value
			.Should()
			.Be("200");

		Context.Set<ProductCharacteristic>().Remove(link);
		await Context.SaveChangesAsync(CancellationToken);
		await Mediator.Publish(
			new Batch<EntityDeletedDomainEvent<ProductCharacteristic, (int, int)>>(
				[new EntityDeletedDomainEvent<ProductCharacteristic, (int, int)>(link.GetId())]),
			CancellationToken);
		var deleted = scope.Flush()
			.Should()
			.ContainSingle()
			.Which
			.Message
			.Should()
			.BeOfType<ProductCharacteristicsUpdatedEvent>()
			.Which;
		deleted.Products.Should().ContainSingle().Which.Characteristics.Should().BeEmpty();
	}

	[Fact]
	public async Task CaptureAll_LoadsSelectedRelationsWithSplitQuery()
	{
		var product = GetContext<ProductTestContext>().Products[0];
		var characteristic =
			await new CharacteristicBuilder(Faker).WithName("Temperature").BuildAndAddToDb(Context);
		await Context.Set<ProductCharacteristic>()
			.AddAsync(
				ProductCharacteristic.Create(
					product.Id,
					characteristic.Id,
					"100"),
				CancellationToken);
		await Context.Set<ProductSize>()
		.AddAsync(
			ProductSize.Create(
				product.Id,
				10,
				20,
				30,
				DimensionUnit.Millimeter),
			CancellationToken);
		await Context.Set<ProductWeight>()
		.AddAsync(
			ProductWeight.Create(
				product.Id,
				2,
				WeightUnit.Kilogram),
			CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		var counter = new QueryCounter();
		var options = new DbContextOptionsBuilder<UnifiedDbContext>()
			.UseNpgsql(Context.Database.GetDbConnection())
			.AddInterceptors(counter)
			.Options;
		await using var queryContext = new UnifiedDbContext(options, [new MainDbContextModule(), new Main.Modules.Identity.Persistence.IdentityDbContextModule(), new Main.Modules.Purchase.Persistence.PurchaseDbContextModule(), new Main.Modules.Sale.Persistence.SaleDbContextModule(), new Main.Modules.Organization.Persistence.OrganizationDbContextModule(), new Main.Modules.Balance.Persistence.BalanceDbContextModule(), new Main.Modules.Analytics.Persistence.AnalyticsDbContextModule(), new Main.Modules.Order.Persistence.OrderDbContextModule(), new Main.Modules.Notifications.Persistence.NotificationsDbContextModule(), new Main.Modules.Cart.Persistence.CartDbContextModule(), new Main.Modules.Documents.Persistence.DocumentsDbContextModule(), new Main.Modules.Jobs.Persistence.JobsDbContextModule(), new CurrencyDbContextModule(), new ProductDbContextModule(), new StorageDbContextModule(), new Pricing.Persistence.PricingDbContextModule()]);
		var provider = new ProductEventProvider(
			new ReadRepository<Product, int>(queryContext),
			Scope.ServiceProvider.GetServices<IProductEventPartProvider>());

		var events = await provider.GetEventsAsync(
			[product.Id],
			ProductEventParts.All,
			CancellationToken);

		counter.Reads.Should().Be(3);
		queryContext.ChangeTracker.Entries().Should().BeEmpty();
		GetEvent<ProductsUpdatedEvent>(events)
			.Products
			.Should()
			.ContainSingle()
			.Which
			.Sku
			.Should()
			.Be(product.Sku.Value);
		GetEvent<ProductCharacteristicsUpdatedEvent>(events)
			.Products
			.Should()
			.ContainSingle()
			.Which
			.Characteristics
			.Should()
			.ContainSingle()
			.Which
			.Value
			.Should()
			.Be("100");
		GetEvent<ProductDimensionsUpdatedEvent>(events).Products.Should().ContainSingle().Which.Dimensions!
			.Length
			.Should()
			.Be(10);
		GetEvent<ProductWeightUpdatedEvent>(events).Products.Should().ContainSingle().Which.Weight!.Value
			.Should()
			.Be(2);
	}

	[Fact]
	public async Task MissingProduct_ProducesDeletionSnapshot()
	{
		var provider = Scope.ServiceProvider.GetRequiredService<IProductEventProvider>();
		var events = await provider.GetEventsAsync(
			[int.MaxValue],
			ProductEventParts.Core,
			CancellationToken);
		var deleted = events.Should()
			.ContainSingle()
			.Which
			.Should()
			.BeOfType<ProductsUpdatedEvent>()
			.Which
			.Products
			.Should()
			.ContainSingle()
			.Which;
		deleted.Id.Should().Be(int.MaxValue);
		deleted.IsDeleted.Should().BeTrue();
	}

	[Fact]
	public async Task ImageUpdate_DoesNotPublishSearchEvents()
	{
		var productId = GetContext<ProductTestContext>().Products[0].Id;
		var scope = Scope.ServiceProvider.GetRequiredService<IIntegrationEventScope>();
		scope.Flush();

		await Mediator.Publish(
			new Batch<ProductImageUpdatedDomainEvent>([new ProductImageUpdatedDomainEvent(productId)]),
			CancellationToken);

		scope.Flush().Should().BeEmpty();
	}

	private static TEvent GetEvent<TEvent>(IReadOnlyList<object> events)
		=> events.OfType<TEvent>().Should().ContainSingle().Which;
}
