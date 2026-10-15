using Application.Common.Models;
using Contracts.Products;
using Enums.Units;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Search.Application.Consumers.Product;
using Search.Application.Handlers.Search.Catalogue;
using Search.Enums;
using Tests.TestContainers.Search;

namespace Search.Tests.Consumers;

public sealed class ProductSnapshotConsumerTests(SearchContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task Consumers_ApplySnapshotsAndExposeProductThroughCatalogueQuery()
	{
		var updatedAt = DateTime.UtcNow;
		var characteristics = new ProductCharacteristicsUpdatedEvent
		{
			UpdatedAt = updatedAt,
			Products =
			[
				new ProductCharacteristicsSnapshot(
					901,
				[
					new ProductCharacteristicSnapshot(10, "12.5")
				])
			]
		};
		await Scope.ServiceProvider.GetRequiredService<ProductCharacteristicsUpdatedConsumer>()
			.Consume(Context(characteristics));
		(await Products.GetById(901, CancellationToken))!.ReadinessStatus.Should().Be(ProductReadinessStatus.Partial);

		var product = new ProductsUpdatedEvent
		{
			UpdatedAt = updatedAt,
			Products = [new ProductSnapshot { Id = 901, Name = "Fixture product", Sku = "FIX-901", ProducerId = 42, Stock = 3 }]
		};
		await Scope.ServiceProvider.GetRequiredService<ProductUpdatedConsumer>().Consume(Context(product));
		await RefreshAsync();

		var result = await Mediator.Send(new SearchInCatalogueQuery(
			"Fixture product",
			new HashSet<SearchTarget> { SearchTarget.Products },
			new HashSet<SearchMatchType>(),
			new HashSet<SearchMatchType> { SearchMatchType.Exact },
			[], new Pagination(0, 20), [], []), CancellationToken);

		(await Products.GetById(901, CancellationToken))!.ReadinessStatus.Should().Be(ProductReadinessStatus.Ready);
		result.Products.Total.Should().Be(1);
		result.Products.Items.Should().ContainSingle().Which.Id.Should().Be(901);
		(await Products.GetById(901, CancellationToken))!.Characteristics.Should()
			.ContainSingle().Which.NumericValue.Should().Be(12.5m);

		var cleared = characteristics with
		{
			UpdatedAt = updatedAt.AddSeconds(1),
			Products = [new ProductCharacteristicsSnapshot(901, [])]
		};
		await Scope.ServiceProvider.GetRequiredService<ProductCharacteristicsUpdatedConsumer>().Consume(Context(cleared));
		(await Products.GetById(901, CancellationToken))!.Characteristics.Should().BeEmpty();
	}

	[Fact]
	public async Task Consumers_ProjectDimensionsAndWeightAndClearRemovedSections()
	{
		var updatedAt = DateTime.UtcNow;
		var dimensions = new ProductDimensionsUpdatedEvent
		{
			UpdatedAt = updatedAt,
			Products = [new(902, new(10, 20, 30, DimensionUnit.Millimeter))]
		};
		var weight = new ProductWeightUpdatedEvent
		{
			UpdatedAt = updatedAt,
			Products = [new(902, new(2, WeightUnit.Kilogram))]
		};
		var dimensionsConsumer = Scope.ServiceProvider.GetRequiredService<ProductDimensionsUpdatedConsumer>();
		var weightConsumer = Scope.ServiceProvider.GetRequiredService<ProductWeightUpdatedConsumer>();

		await dimensionsConsumer.Consume(Context(dimensions));
		await weightConsumer.Consume(Context(weight));

		var product = await Products.GetById(902, CancellationToken);
		product!.Dimensions!.LengthM.Should().Be(0.01m);
		product.Dimensions.VolumeM3.Should().Be(0.000006m);
		product.Weight!.WeightKg.Should().Be(2);
		product.ReadinessStatus.Should().Be(ProductReadinessStatus.Partial);

		await dimensionsConsumer.Consume(Context(new ProductDimensionsUpdatedEvent
		{
			UpdatedAt = updatedAt.AddSeconds(1),
			Products = [new(902, null)]
		}));
		await weightConsumer.Consume(Context(new ProductWeightUpdatedEvent
		{
			UpdatedAt = updatedAt.AddSeconds(1),
			Products = [new(902, null)]
		}));

		product = await Products.GetById(902, CancellationToken);
		product!.Dimensions.Should().BeNull();
		product.Weight.Should().BeNull();
	}

	[Fact]
	public async Task DeletedCoreEvent_RemainsDeletedAfterPartialEvents()
	{
		var updatedAt = DateTime.UtcNow;
		await Scope.ServiceProvider.GetRequiredService<ProductUpdatedConsumer>().Consume(Context(new ProductsUpdatedEvent
		{
			UpdatedAt = updatedAt,
			Products = [new ProductSnapshot { Id = 903, IsDeleted = true }]
		}));

		await Scope.ServiceProvider.GetRequiredService<ProductCharacteristicsUpdatedConsumer>()
			.Consume(Context(new ProductCharacteristicsUpdatedEvent
			{
				UpdatedAt = updatedAt.AddSeconds(1),
				Products = [new(903, [new(10, "12.5")])]
			}));
		await Scope.ServiceProvider.GetRequiredService<ProductWeightUpdatedConsumer>().Consume(Context(new ProductWeightUpdatedEvent
		{
			UpdatedAt = updatedAt.AddSeconds(1),
			Products = [new(903, new(2, WeightUnit.Kilogram))]
		}));

		var product = await Products.GetById(903, CancellationToken);
		product!.ReadinessStatus.Should().Be(ProductReadinessStatus.Deleted);
		product.ProductUpdatedAt.Should().Be(updatedAt);
		product.Characteristics.Should().ContainSingle().Which.NumericValue.Should().Be(12.5m);
		product.Weight!.WeightKg.Should().Be(2);
	}

	[Fact]
	public async Task ReadyProduct_RemainsReadyAndSearchableAfterPartialUpdatesAndClears()
	{
		var updatedAt = DateTime.UtcNow;
		const int productId = 904;
		await Scope.ServiceProvider.GetRequiredService<ProductUpdatedConsumer>().Consume(Context(new ProductsUpdatedEvent
		{
			UpdatedAt = updatedAt,
			Products = [new ProductSnapshot { Id = productId, Name = "Ready product", Sku = "READY-904", ProducerId = 42, Stock = 3 }]
		}));
		await AssertReadyAndSearchable();

		var dimensionsConsumer = Scope.ServiceProvider.GetRequiredService<ProductDimensionsUpdatedConsumer>();
		var weightConsumer = Scope.ServiceProvider.GetRequiredService<ProductWeightUpdatedConsumer>();
		var characteristicsConsumer = Scope.ServiceProvider.GetRequiredService<ProductCharacteristicsUpdatedConsumer>();

		await dimensionsConsumer.Consume(Context(new ProductDimensionsUpdatedEvent
		{
			UpdatedAt = updatedAt.AddSeconds(1),
			Products = [new(productId, new(10, 20, 30, DimensionUnit.Millimeter))]
		}));
		var product = await AssertReadyAndSearchable();
		product.Dimensions!.LengthM.Should().Be(0.01m);
		product.DimensionsUpdatedAt.Should().Be(updatedAt.AddSeconds(1));

		await weightConsumer.Consume(Context(new ProductWeightUpdatedEvent
		{
			UpdatedAt = updatedAt.AddSeconds(1),
			Products = [new(productId, new(2, WeightUnit.Kilogram))]
		}));
		product = await AssertReadyAndSearchable();
		product.Weight!.WeightKg.Should().Be(2);
		product.WeightUpdatedAt.Should().Be(updatedAt.AddSeconds(1));
		product.Dimensions!.LengthM.Should().Be(0.01m);

		await characteristicsConsumer.Consume(Context(new ProductCharacteristicsUpdatedEvent
		{
			UpdatedAt = updatedAt.AddSeconds(1),
			Products = [new(productId, [new(10, "12.5")])]
		}));
		product = await AssertReadyAndSearchable();
		product.Characteristics.Should().ContainSingle().Which.NumericValue.Should().Be(12.5m);
		product.CharacteristicsUpdatedAt.Should().Be(updatedAt.AddSeconds(1));
		product.Weight!.WeightKg.Should().Be(2);
		product.Dimensions!.LengthM.Should().Be(0.01m);

		await dimensionsConsumer.Consume(Context(new ProductDimensionsUpdatedEvent
		{
			UpdatedAt = updatedAt.AddSeconds(2), Products = [new(productId, null)]
		}));
		(await AssertReadyAndSearchable()).Dimensions.Should().BeNull();

		await weightConsumer.Consume(Context(new ProductWeightUpdatedEvent
		{
			UpdatedAt = updatedAt.AddSeconds(2), Products = [new(productId, null)]
		}));
		(await AssertReadyAndSearchable()).Weight.Should().BeNull();

		await characteristicsConsumer.Consume(Context(new ProductCharacteristicsUpdatedEvent
		{
			UpdatedAt = updatedAt.AddSeconds(2), Products = [new(productId, [])]
		}));
		(await AssertReadyAndSearchable()).Characteristics.Should().BeEmpty();

		async Task<Search.Entities.Product> AssertReadyAndSearchable()
		{
			var document = await Products.GetById(productId, CancellationToken);
			document.Should().NotBeNull();
			document!.ReadinessStatus.Should().Be(ProductReadinessStatus.Ready);
			document.ProductUpdatedAt.Should().Be(updatedAt);
			document.Name.Should().Be("Ready product");
			document.Sku.Should().Be("READY-904");
			document.ProducerId.Should().Be(42);
			document.Stock.Should().Be(3);

			await RefreshAsync();
			var result = await Mediator.Send(new SearchInCatalogueQuery(
				"Ready product", new HashSet<SearchTarget> { SearchTarget.Products },
				new HashSet<SearchMatchType>(), new HashSet<SearchMatchType> { SearchMatchType.Exact },
				[], new Pagination(0, 20), [], []), CancellationToken);
			result.Products.Total.Should().Be(1);
			result.Products.Items.Should().ContainSingle().Which.Id.Should().Be(productId);
			return document;
		}
	}

	private ConsumeContext<T> Context<T>(T message) where T : class
	{
		var context = new Mock<ConsumeContext<T>>();
		context.SetupGet(x => x.Message).Returns(message);
		context.SetupGet(x => x.CancellationToken).Returns(CancellationToken);
		return context.Object;
	}
}
