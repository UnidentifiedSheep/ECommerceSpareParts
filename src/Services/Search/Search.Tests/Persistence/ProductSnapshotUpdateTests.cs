using Application.Common.Models;
using Enums.Units;
using FluentAssertions;
using Search.Application.Models.CatalogueSearch;
using Search.Application.Models.ProductUpdates;
using Search.Application.Interfaces.Product;
using Tests.TestContainers.Search;
using Search.Entities;
using Search.Enums;

namespace Search.Tests.Persistence;

public sealed class ProductSnapshotUpdateTests(SearchContainerFixture fixture) : IntegrationTest(fixture)
{
	private static CancellationToken Token => TestContext.Current.CancellationToken;
	private static readonly DateTime Timestamp = new(2026, 10, 14, 8, 0, 0, DateTimeKind.Utc);

	[Fact]
	public async Task OutOfOrderSnapshots_PreserveIndependentFieldsAndHideIncompleteProducts()
	{
		await Products.ApplyUpdates([new ProductCharacteristicsUpdate
		{
			ProductId = 123, UpdatedAt = Timestamp.AddSeconds(3), Characteristics = [new(10, "12.5", 12.5m)]
		}], Token);
		(await Products.GetById(123, Token))!.ReadinessStatus.Should().Be(ProductReadinessStatus.Partial);
		var partial = await Client.GetAsync<Dictionary<string, object>>(123, request => request.Index(Indexes.Products), Token);
		partial.IsValid.Should().BeTrue(partial.DebugInformation);
		partial.Source.Should().ContainKey("readinessStatus");
		Convert.ToInt32(partial.Source["readinessStatus"]).Should().Be((int)ProductReadinessStatus.Partial);
		await RefreshAsync();
		(await Search(Products)).Total.Should().Be(0);

		await Products.ApplyUpdates([Product()], Token);
		await Products.ApplyUpdates([new ProductCharacteristicsUpdate
		{
			ProductId = 123, UpdatedAt = Timestamp.AddSeconds(2), Characteristics = [new(10, "999", 999)]
		}], Token);
		await Products.ApplyUpdates([Product() with { Stock = 8, UpdatedAt = Timestamp.AddSeconds(4) }], Token);
		await RefreshAsync();

		var result = await Products.GetById(123, Token);
		result!.ReadinessStatus.Should().Be(ProductReadinessStatus.Ready);
		result.Stock.Should().Be(8);
		result.Characteristics.Should().ContainSingle().Which.NumericValue.Should().Be(12.5m);
		result.CharacteristicsUpdatedAt.Should().Be(Timestamp.AddSeconds(3));
		(await Search(Products)).Total.Should().Be(1);

		await Products.ApplyUpdates([new ProductCharacteristicsUpdate
		{
			ProductId = 123, UpdatedAt = Timestamp.AddSeconds(5), Characteristics = []
		}], Token);
		(await Products.GetById(123, Token))!.Characteristics.Should().BeEmpty();
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task DeletedProduct_CannotBeResurrectedByAnOlderSnapshot(bool productExisted)
	{
		if (productExisted) await Products.ApplyUpdates([Product()], Token);
		await Products.ApplyUpdates([new ProductCoreUpdate
		{
			ProductId = 123, UpdatedAt = Timestamp.AddSeconds(2), ReadinessStatus = ProductReadinessStatus.Deleted
		}], Token);
		await Products.ApplyUpdates([Product() with { UpdatedAt = Timestamp.AddSeconds(1) }], Token);
		await Products.ApplyUpdates([
			new ProductCharacteristicsUpdate
			{
				ProductId = 123, UpdatedAt = Timestamp.AddSeconds(3), Characteristics = [new(10, "12.5", 12.5m)]
			},
			new ProductDimensionsUpdate
			{
				ProductId = 123, UpdatedAt = Timestamp.AddSeconds(3),
				Dimensions = new ProductDimensions { Length = 10, Width = 20, Height = 30, Unit = DimensionUnit.Millimeter }
			},
			new ProductWeightUpdate
			{
				ProductId = 123, UpdatedAt = Timestamp.AddSeconds(3),
				Weight = new ProductWeight { Value = 2, Unit = WeightUnit.Kilogram, WeightKg = 2 }
			}
		], Token);
		await RefreshAsync();
		(await Search(Products)).Total.Should().Be(0);
		(await Products.GetById(123, Token))!.ReadinessStatus.Should().Be(ProductReadinessStatus.Deleted);
	}

	[Fact]
	public async Task NullableSnapshots_ClearDimensionsWeightAndProductGroup()
	{
		await Products.ApplyUpdates([
			Product(),
			new ProductDimensionsUpdate
			{
				ProductId = 123, UpdatedAt = Timestamp,
				Dimensions = new ProductDimensions { Length = 10, Width = 20, Height = 30, Unit = DimensionUnit.Millimeter }
			},
			new ProductWeightUpdate
			{
				ProductId = 123, UpdatedAt = Timestamp,
				Weight = new ProductWeight { Value = 2, Unit = WeightUnit.Kilogram, WeightKg = 2 }
			}
		], Token);
		(await Products.GetById(123, Token))!.Weight!.WeightKg.Should().Be(2);
		await Products.ApplyUpdates([
			Product() with { ProductGroupId = null, Indicator = null, UpdatedAt = Timestamp.AddTicks(1) },
			new ProductDimensionsUpdate { ProductId = 123, UpdatedAt = Timestamp.AddTicks(1), Dimensions = null },
			new ProductWeightUpdate { ProductId = 123, UpdatedAt = Timestamp.AddTicks(1), Weight = null }
		], Token);
		var result = await Products.GetById(123, Token);
		result!.Weight.Should().BeNull();
		result.Dimensions.Should().BeNull();
		result.ProductGroupId.Should().BeNull();
		result.Indicator.Should().BeNull();
	}

	[Fact]
	public async Task NestedNumericFilter_MatchesValueOfTheRequestedCharacteristic()
	{
		await Products.ApplyUpdates([
			Product(),
			new ProductCharacteristicsUpdate
			{
				ProductId = 123, UpdatedAt = Timestamp, Characteristics = [new(10, "100", 100), new(20, "12.5", 12.5m)]
			}
		], Token);
		await RefreshAsync();
		async Task<long> Find(int characteristicId)
		{
			var response = await Client.SearchAsync<Product>(s => s.Index(Indexes.Products).Query(q => q.Nested(n => n
				.Path("characteristics").Query(nq => nq.Bool(b => b.Filter(
					f => f.Term("characteristics.characteristicId", characteristicId),
					f => f.Range(r => r.Field("characteristics.numericValue").GreaterThanOrEquals(10).LessThanOrEquals(15))))))), Token);
			response.IsValid.Should().BeTrue(response.DebugInformation);
			return response.Total;
		}
		(await Find(10)).Should().Be(0);
		(await Find(20)).Should().Be(1);
	}

	[Fact]
	public async Task UpdateMetadata_IsNotStoredInTheDocument()
	{
		await Products.ApplyUpdates([Product(), new ProductWeightUpdate
		{
			ProductId = 123, UpdatedAt = Timestamp, Weight = new ProductWeight { Value = 2, Unit = WeightUnit.Kilogram, WeightKg = 2 }
		}], Token);

		var response = await Client.GetAsync<Dictionary<string, object>>(123, request => request.Index(Indexes.Products), Token);

		response.IsValid.Should().BeTrue(response.DebugInformation);
		response.Source.Keys.Should().Contain(["id", "productUpdatedAt", "weightUpdatedAt", "weight"]);
		response.Source.Keys.Should().NotContain(["productId", "updatedAt", "updatedAtField", "payload", "fields"]);
	}

	private static ProductCoreUpdate Product() => new()
	{
		ProductId = 123, UpdatedAt = Timestamp, Sku = "TEST-123", NormalizedSku = "test123", Name = "Test product",
		ProducerId = 1, ProductGroupId = 7, Stock = 1, Indicator = "test", ReadinessStatus = ProductReadinessStatus.Ready
	};

	private static Task<SearchResult<Product>> Search(IProductRepository repository) => repository.Search(new CatalogueSearchCriteria
	{
		Query = "", SkuModes = new HashSet<SearchMatchType>(), NameModes = new HashSet<SearchMatchType>(), ProducerIds = [], Pagination = new Pagination(0, 20)
	}, Token);
}
