using FluentAssertions;
using System.Data.Common;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.Products;
using Domain.Events;
using Enums.Units;
using Main.Application.Interfaces.Products;
using Main.Application.Services.Products;
using Main.Entities.DomainEvents.Product;
using Main.Entities.Product;
using Main.Enums;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Characteristics;

public class ProductSearchEventTests : IntegrationTest
{
	public ProductSearchEventTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
	}

	[Fact]
	public async Task CaptureAll_ReturnsProductAndEmptyNullableSectionsWithoutChangingProduct()
	{
		var product = GetContext<ProductTestContext>().Products[0];
		var provider = Scope.ServiceProvider.GetRequiredService<IProductEventProvider>();
		var version = product.RowVersion;
		var events = await provider.GetEventsAsync([product.Id, product.Id], ProductEventParts.All, CancellationToken);

		events.Should().HaveCount(4);
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
		product.RowVersion.Should().Be(version);
	}

	[Fact]
	public async Task DeletingLastCharacteristic_ProducesAnEmptySnapshot()
	{
		var productId = GetContext<ProductTestContext>().Products[0].Id;
		var characteristic = await new CharacteristicBuilder(Faker).WithName("Temperature").BuildAndAddToDb(Context);
		var link = ProductCharacteristic.Create(productId, characteristic.Id, "100");
		await Context.ProductCharacteristics.AddAsync(link, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		var provider = Scope.ServiceProvider.GetRequiredService<IProductEventProvider>();
		var initial = GetEvent<ProductCharacteristicsUpdatedEvent>(
			await provider.GetEventsAsync([productId], ProductEventParts.Characteristics, CancellationToken));
		initial.Products.Should().ContainSingle().Which.Characteristics.Should().ContainSingle().Which.Value.Should().Be("100");

		Context.ProductCharacteristics.Remove(link);
		await Context.SaveChangesAsync(CancellationToken);
		var notification = GetEvent<ProductCharacteristicsUpdatedEvent>(
			await provider.GetEventsAsync([productId], ProductEventParts.Characteristics, CancellationToken));

		notification.Products.Should().ContainSingle().Which.Characteristics.Should().BeEmpty();
		notification.UpdatedAt.Should().BeAfter(initial.UpdatedAt);
	}

	[Fact]
	public async Task LifecycleBatches_PublishCurrentCharacteristicsSnapshot()
	{
		var productId = GetContext<ProductTestContext>().Products[0].Id;
		var characteristic = await new CharacteristicBuilder(Faker).WithName("Temperature").BuildAndAddToDb(Context);
		var link = ProductCharacteristic.Create(productId, characteristic.Id, "100");
		await Context.ProductCharacteristics.AddAsync(link, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		var scope = Scope.ServiceProvider.GetRequiredService<IIntegrationEventScope>();
		scope.Flush();

		await Mediator.Publish(new Batch<EntityCreatedDomainEvent<ProductCharacteristic>>(
			[new(link), new(link)]), CancellationToken);
		var created = scope.Flush().Should().ContainSingle().Which.Message.Should()
			.BeOfType<ProductCharacteristicsUpdatedEvent>().Which;
		created.Products.Should().ContainSingle().Which.Characteristics.Should().ContainSingle().Which.Value.Should().Be("100");

		link.SetValue("200");
		await Context.SaveChangesAsync(CancellationToken);
		await Mediator.Publish(new Batch<EntityUpdatedDomainEvent<ProductCharacteristic, (int, int)>>(
			[new(link.GetId())]), CancellationToken);
		var updated = scope.Flush().Should().ContainSingle().Which.Message.Should()
			.BeOfType<ProductCharacteristicsUpdatedEvent>().Which;
		updated.Products.Should().ContainSingle().Which.Characteristics.Should().ContainSingle().Which.Value.Should().Be("200");

		Context.ProductCharacteristics.Remove(link);
		await Context.SaveChangesAsync(CancellationToken);
		await Mediator.Publish(new Batch<EntityDeletedDomainEvent<ProductCharacteristic, (int, int)>>(
			[new(link.GetId())]), CancellationToken);
		var deleted = scope.Flush().Should().ContainSingle().Which.Message.Should()
			.BeOfType<ProductCharacteristicsUpdatedEvent>().Which;
		deleted.Products.Should().ContainSingle().Which.Characteristics.Should().BeEmpty();
	}

	[Fact]
	public async Task CaptureAll_LoadsSelectedRelationsInOneQuery()
	{
		var product = GetContext<ProductTestContext>().Products[0];
		var characteristic = await new CharacteristicBuilder(Faker).WithName("Temperature").BuildAndAddToDb(Context);
		await Context.ProductCharacteristics.AddAsync(ProductCharacteristic.Create(product.Id, characteristic.Id, "100"), CancellationToken);
		await Context.ProductSizes.AddAsync(ProductSize.Create(product.Id, 10, 20, 30, DimensionUnit.Millimeter), CancellationToken);
		await Context.ProductWeights.AddAsync(ProductWeight.Create(product.Id, 2, WeightUnit.Kilogram), CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		var counter = new QueryCounter();
		var options = new DbContextOptionsBuilder<DContext>()
			.UseNpgsql(Context.Database.GetDbConnection())
			.AddInterceptors(counter)
			.Options;
		await using var queryContext = new DContext(options);
		var provider = new ProductEventProvider(
			new Main.Persistence.ReadRepository<Main.Entities.Product.Product, int>(queryContext),
			Scope.ServiceProvider.GetServices<IProductEventPartProvider>());

		var events = await provider.GetEventsAsync([product.Id], ProductEventParts.All, CancellationToken);

		counter.Reads.Should().Be(1);
		queryContext.ChangeTracker.Entries().Should().BeEmpty();
		GetEvent<ProductsUpdatedEvent>(events).Products.Should().ContainSingle().Which.Sku.Should().Be(product.Sku.Value);
		GetEvent<ProductCharacteristicsUpdatedEvent>(events).Products.Should().ContainSingle().Which.Characteristics.Should()
			.ContainSingle().Which.Value.Should().Be("100");
		GetEvent<ProductDimensionsUpdatedEvent>(events).Products.Should().ContainSingle().Which.Dimensions!.Length.Should().Be(10);
		GetEvent<ProductWeightUpdatedEvent>(events).Products.Should().ContainSingle().Which.Weight!.Value.Should().Be(2);
	}

	[Fact]
	public async Task MissingProduct_ProducesDeletionSnapshot()
	{
		var provider = Scope.ServiceProvider.GetRequiredService<IProductEventProvider>();
		var events = await provider.GetEventsAsync([int.MaxValue], ProductEventParts.Core, CancellationToken);
		var deleted = events.Should().ContainSingle().Which.Should().BeOfType<ProductsUpdatedEvent>().Which
			.Products.Should().ContainSingle().Which;
		deleted.Id.Should().Be(int.MaxValue);
		deleted.IsDeleted.Should().BeTrue();
	}

	[Fact]
	public async Task ImageUpdate_DoesNotPublishSearchEvents()
	{
		var productId = GetContext<ProductTestContext>().Products[0].Id;
		var scope = Scope.ServiceProvider.GetRequiredService<IIntegrationEventScope>();
		scope.Flush();

		await Mediator.Publish(new Batch<ProductImageUpdatedDomainEvent>([new(productId)]), CancellationToken);

		scope.Flush().Should().BeEmpty();
	}

	private static TEvent GetEvent<TEvent>(IReadOnlyList<object> events) =>
		events.OfType<TEvent>().Should().ContainSingle().Which;

	private sealed class QueryCounter : DbCommandInterceptor
	{
		public int Reads { get; private set; }

		public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
			DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
			CancellationToken cancellationToken = default)
		{
			Reads++;
			return ValueTask.FromResult(result);
		}
	}
}
