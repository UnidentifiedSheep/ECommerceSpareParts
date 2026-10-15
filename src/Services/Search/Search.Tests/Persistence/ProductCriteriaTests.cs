using Application.Common.Models;
using Enums.Units;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Search.Application.Handlers.Search.Catalogue;
using Search.Application.Interfaces.CatalogueCandidate;
using Search.Application.Models.CatalogueSearch.Criteria;
using Search.Entities;
using Search.Enums;
using Search.Tests.TestData;
using Tests.TestContainers.Search;

namespace Search.Tests.Persistence;

public sealed class ProductCriteriaTests(SearchContainerFixture fixture) : ProductCriteriaTestBase(fixture)
{
	[Fact]
	public async Task Characteristics_UseAndAcrossIdsAndOrWithinValuesWithoutMatchingAnotherCharacteristicsNumber()
	{
		var ids = await FindAsync(new ProductSearchCriteria
		{
			Characteristics =
			[
				new() { CharacteristicId = 10, NumericRange = new() { Gte = 10, Lte = 20 } },
				new() { CharacteristicId = 20, Values = ["M10", "M12"] }
			]
		});
		ids.Should().Equal(101);
	}

	[Fact]
	public async Task Values_MatchAnySelectedValueAndKeepPartialAndDeletedProductsHidden()
	{
		(await FindAsync(new ProductSearchCriteria
		{
			Characteristics = [new() { CharacteristicId = 20, Values = ["M10", "M12"] }]
		})).Should().Equal(101, 102, 103);
		(await FindAsync(new ProductSearchCriteria
		{
			Characteristics = [new() { CharacteristicId = 10, Values = ["not a number"] }]
		})).Should().Equal(103);
	}

	[Theory]
	[InlineData(false, false, "101,102")]
	[InlineData(true, false, "102")]
	[InlineData(false, true, "101")]
	[InlineData(true, true, "")]
	public async Task NumericRanges_RespectInclusiveAndExclusiveBounds(bool lowerExclusive, bool upperExclusive, string expected)
	{
		var ids = await FindAsync(new ProductSearchCriteria
		{
			Characteristics = [new()
			{
				CharacteristicId = 10,
				NumericRange = new()
				{
					Gt = lowerExclusive ? 12.5m : null, Gte = lowerExclusive ? null : 12.5m,
					Lt = upperExclusive ? 100m : null, Lte = upperExclusive ? null : 100m
				}
			}]
		});
		string.Join(",", ids).Should().Be(expected);
	}

	[Fact]
	public async Task OneSidedRange_SupportsNegativeValuesAndExcludesMissingAndNonnumericValues()
	{
		(await FindAsync(new ProductSearchCriteria
		{
			Characteristics = [new() { CharacteristicId = 40, NumericRange = new() { Lt = 0 } }]
		})).Should().Equal(101);
		(await FindAsync(new ProductSearchCriteria
		{
			Characteristics = [new() { CharacteristicId = 10, NumericRange = new() { Lte = 12.5m } }]
		})).Should().Equal(101);
	}

	[Fact]
	public async Task ExistsFalse_ExcludesTheWholeProductWhenTheCharacteristicIsPresent()
	{
		(await FindAsync(new ProductSearchCriteria
		{
			Characteristics = [new() { CharacteristicId = 30, Exists = false }]
		})).Should().Equal(102, 103);
		(await FindAsync(new ProductSearchCriteria
		{
			Characteristics = [new() { CharacteristicId = 30, Exists = true }]
		})).Should().Equal(101);
	}

	[Fact]
	public async Task MeasurementRanges_NormalizeUnitsAndPreserveExactBoundaries()
	{
		(await FindAsync(new ProductSearchCriteria
		{
			Weight = new() { Unit = WeightUnit.Gram, Range = new() { Gte = 2000, Lte = 2000 } },
			Dimensions = new()
			{
				Unit = DimensionUnit.Millimeter,
				Length = new() { Gte = 10, Lte = 10 }, Width = new() { Gte = 20, Lte = 20 },
				Height = new() { Gte = 30, Lte = 30 }, Volume = new() { Gte = 6000, Lte = 6000 }
			}
		})).Should().Equal(101);
		(await FindAsync(new ProductSearchCriteria
		{
			Weight = new() { Unit = WeightUnit.Kilogram, Range = new() { Gte = 2, Lte = 2 } },
			Dimensions = new() { Unit = DimensionUnit.Centimeter, Length = new() { Gte = 1, Lte = 1 } }
		})).Should().Equal(101);
	}

	[Fact]
	public async Task MissingMeasurements_DoNotMatchARange()
	{
		(await FindAsync(new ProductSearchCriteria
		{
			Weight = new() { Range = new() { Gte = 0 } }
		})).Should().Equal(101, 102);
		(await FindAsync(new ProductSearchCriteria
		{
			Dimensions = new() { Length = new() { Gt = 0 } }
		})).Should().Equal(101, 102);
	}

	[Fact]
	public async Task StockAndText_CombineWithAnd_AndFiltersWorkWithoutText()
	{
		var criteria = new ProductSearchCriteria { Stock = new() { Gt = 0 } };
		(await FindAsync(criteria)).Should().Equal(101, 103);
		(await FindAsync(criteria, "Oil")).Should().Equal(101);
		(await FindAsync(criteria, "Air")).Should().BeEmpty();
	}

	[Fact]
	public async Task VeryLargeMeasurementBound_DoesNotOverflowDuringNormalization()
	{
		(await FindAsync(new ProductSearchCriteria
		{
			Weight = new() { Unit = WeightUnit.Tonne, Range = new() { Gte = decimal.MaxValue } }
		})).Should().BeEmpty();
	}

	[Fact]
	public async Task ProductCriteria_DoNotFilterCatalogueCandidates()
	{
		await Scope.ServiceProvider.GetRequiredService<ICatalogueCandidateRepository>().Upsert(new CatalogueCandidate
		{
			Id = Guid.NewGuid(), Sku = "CAND-1", NormalizedSku = "cand1", ProducerId = 42, MappedProductId = null, Names = ["Candidate"]
		}, CancellationToken);
		await RefreshAsync();
		var result = await Mediator.Send(new SearchInCatalogueQuery(null,
			new HashSet<SearchTarget> { SearchTarget.Products, SearchTarget.CatalogueCandidates },
			new HashSet<SearchMatchType>(), new HashSet<SearchMatchType>(), [], new Pagination(0, 20), [], [],
			ProductCriteria: new() { Stock = new() { Gt = 1000 } }), CancellationToken);
		result.Products.Total.Should().Be(0);
		result.SearchInCatalogueCandidates.Total.Should().Be(1);
	}
}
