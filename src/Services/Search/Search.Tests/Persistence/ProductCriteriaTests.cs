using Application.Common.Models;
using Enums.Units;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Search.Application.Interfaces.CatalogueCandidate;
using Search.Application.Models.CatalogueSearch.Criteria;
using Search.Application.Queries.Search.Catalogue;
using Search.Entities;
using Search.Enums;
using Search.Tests.TestData;
using Tests.TestContainers.Search;

namespace Search.Tests.Persistence;

public sealed class ProductCriteriaTests(SearchContainerFixture fixture) : ProductCriteriaTestBase(fixture)
{
	[Fact]
	public async Task
		Characteristics_UseAndAcrossIdsAndOrWithinValuesWithoutMatchingAnotherCharacteristicsNumber()
	{
		var ids = await FindAsync(
			new ProductSearchCriteria
			{
				Characteristics =
				[
					new CharacteristicSearchCriteria
					{
						CharacteristicId = 10,
						NumericRange = new RangeCriteria<decimal> { Gte = 10, Lte = 20 }
					},
					new CharacteristicSearchCriteria { CharacteristicId = 20, Values = ["M10", "M12"] }
				]
			});
		ids.Should().Equal(101);
	}

	[Fact]
	public async Task Values_MatchAnySelectedValueAndKeepPartialAndDeletedProductsHidden()
	{
		(await FindAsync(
				new ProductSearchCriteria
				{
					Characteristics =
						[new CharacteristicSearchCriteria { CharacteristicId = 20, Values = ["M10", "M12"] }]
				})).Should()
			.Equal(
				101,
				102,
				103);
		(await FindAsync(
				new ProductSearchCriteria
				{
					Characteristics =
					[
						new CharacteristicSearchCriteria { CharacteristicId = 10, Values = ["not a number"] }
					]
				})).Should()
			.Equal(103);
	}

	[Theory]
	[InlineData(
		false,
		false,
		"101,102")]
	[InlineData(
		true,
		false,
		"102")]
	[InlineData(
		false,
		true,
		"101")]
	[InlineData(
		true,
		true,
		"")]
	public async Task NumericRanges_RespectInclusiveAndExclusiveBounds(
		bool lowerExclusive,
		bool upperExclusive,
		string expected)
	{
		var ids = await FindAsync(
			new ProductSearchCriteria
			{
				Characteristics =
				[
					new CharacteristicSearchCriteria
					{
						CharacteristicId = 10,
						NumericRange = new RangeCriteria<decimal>
						{
							Gt = lowerExclusive ? 12.5m : null, Gte = lowerExclusive ? null : 12.5m,
							Lt = upperExclusive ? 100m : null, Lte = upperExclusive ? null : 100m
						}
					}
				]
			});
		string.Join(",", ids).Should().Be(expected);
	}

	[Fact]
	public async Task OneSidedRange_SupportsNegativeValuesAndExcludesMissingAndNonnumericValues()
	{
		(await FindAsync(
				new ProductSearchCriteria
				{
					Characteristics =
					[
						new CharacteristicSearchCriteria
							{ CharacteristicId = 40, NumericRange = new RangeCriteria<decimal> { Lt = 0 } }
					]
				})).Should()
			.Equal(101);
		(await FindAsync(
				new ProductSearchCriteria
				{
					Characteristics =
					[
						new CharacteristicSearchCriteria
						{
							CharacteristicId = 10, NumericRange = new RangeCriteria<decimal> { Lte = 12.5m }
						}
					]
				})).Should()
			.Equal(101);
	}

	[Fact]
	public async Task ExistsFalse_ExcludesTheWholeProductWhenTheCharacteristicIsPresent()
	{
		(await FindAsync(
				new ProductSearchCriteria
				{
					Characteristics =
						[new CharacteristicSearchCriteria { CharacteristicId = 30, Exists = false }]
				})).Should()
			.Equal(102, 103);
		(await FindAsync(
				new ProductSearchCriteria
				{
					Characteristics =
						[new CharacteristicSearchCriteria { CharacteristicId = 30, Exists = true }]
				})).Should()
			.Equal(101);
	}

	[Fact]
	public async Task MeasurementRanges_NormalizeUnitsAndPreserveExactBoundaries()
	{
		(await FindAsync(
				new ProductSearchCriteria
				{
					Weight = new WeightSearchCriteria
					{
						Unit = WeightUnit.Gram, Range = new RangeCriteria<decimal> { Gte = 2000, Lte = 2000 }
					},
					Dimensions = new DimensionsSearchCriteria
					{
						Unit = DimensionUnit.Millimeter,
						Length = new RangeCriteria<decimal> { Gte = 10, Lte = 10 },
						Width = new RangeCriteria<decimal> { Gte = 20, Lte = 20 },
						Height = new RangeCriteria<decimal> { Gte = 30, Lte = 30 },
						Volume = new RangeCriteria<decimal> { Gte = 6000, Lte = 6000 }
					}
				})).Should()
			.Equal(101);
		(await FindAsync(
				new ProductSearchCriteria
				{
					Weight = new WeightSearchCriteria
					{
						Unit = WeightUnit.Kilogram, Range = new RangeCriteria<decimal> { Gte = 2, Lte = 2 }
					},
					Dimensions = new DimensionsSearchCriteria
					{
						Unit = DimensionUnit.Centimeter,
						Length = new RangeCriteria<decimal> { Gte = 1, Lte = 1 }
					}
				})).Should()
			.Equal(101);
	}

	[Fact]
	public async Task MissingMeasurements_DoNotMatchARange()
	{
		(await FindAsync(
				new ProductSearchCriteria
				{
					Weight = new WeightSearchCriteria { Range = new RangeCriteria<decimal> { Gte = 0 } }
				})).Should()
			.Equal(101, 102);
		(await FindAsync(
				new ProductSearchCriteria
				{
					Dimensions = new DimensionsSearchCriteria
						{ Length = new RangeCriteria<decimal> { Gt = 0 } }
				})).Should()
			.Equal(101, 102);
	}

	[Fact]
	public async Task CharacteristicsAndText_CombineWithAnd_AndFiltersWorkWithoutText()
	{
		var criteria = new ProductSearchCriteria
		{
			Characteristics = [new CharacteristicSearchCriteria { CharacteristicId = 20, Values = ["M10"] }]
		};
		(await FindAsync(criteria)).Should().Equal(101);
		(await FindAsync(criteria, "Oil")).Should().Equal(101);
		(await FindAsync(criteria, "Air")).Should().BeEmpty();
	}

	[Fact]
	public async Task VeryLargeMeasurementBound_DoesNotOverflowDuringNormalization()
		=> (await FindAsync(
				new ProductSearchCriteria
				{
					Weight = new WeightSearchCriteria
					{
						Unit = WeightUnit.Tonne, Range = new RangeCriteria<decimal> { Gte = decimal.MaxValue }
					}
				})).Should()
			.BeEmpty();

	[Fact]
	public async Task ProductCriteria_DoNotFilterCatalogueCandidates()
	{
		await Scope.ServiceProvider
			.GetRequiredService<ICatalogueCandidateRepository>()
			.Upsert(
				new CatalogueCandidate
				{
					Id = Guid.NewGuid(), Sku = "CAND-1", NormalizedSku = "cand1", ProducerId = 42,
					MappedProductId = null, Names = ["Candidate"]
				},
				CancellationToken);
		await RefreshAsync();
		var result = await Mediator.Send(
			new SearchInCatalogueQuery(
				null,
				new HashSet<SearchTarget> { SearchTarget.Products, SearchTarget.CatalogueCandidates },
				new HashSet<SearchMatchType>(),
				new HashSet<SearchMatchType>(),
				[],
				new Pagination(0, 20),
				[],
				[],
				ProductCriteria: new ProductSearchCriteria
				{
					Weight = new WeightSearchCriteria { Range = new RangeCriteria<decimal> { Gt = 1000 } }
				}),
			CancellationToken);
		result.Products.Total.Should().Be(0);
		result.SearchInCatalogueCandidates.Total.Should().Be(1);
	}
}
