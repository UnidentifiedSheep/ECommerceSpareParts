using Application.Common.Models;
using Enums.Units;
using FluentAssertions;
using Search.Application.Handlers.Search.Catalogue;
using Search.Application.Models.CatalogueSearch.Criteria;
using Search.Enums;

namespace Search.Tests.Handlers.Catalogue;

public sealed class ProductSearchCriteriaValidationTests
{
	private readonly SearchInCatalogueValidation _validator = new();

	[Theory]
	[InlineData("empty")]
	[InlineData("twoLowerBounds")]
	[InlineData("twoUpperBounds")]
	[InlineData("reversed")]
	[InlineData("emptyExclusiveLower")]
	[InlineData("emptyExclusiveUpper")]
	public async Task InvalidRange_IsRejected(string scenario)
	{
		var range = scenario switch
		{
			"empty" => new RangeCriteria<decimal>(),
			"twoLowerBounds" => new RangeCriteria<decimal> { Gt = 1, Gte = 1 },
			"twoUpperBounds" => new RangeCriteria<decimal> { Lt = 2, Lte = 2 },
			"reversed" => new RangeCriteria<decimal> { Gte = 2, Lte = 1 },
			"emptyExclusiveLower" => new RangeCriteria<decimal> { Gt = 1, Lte = 1 },
			"emptyExclusiveUpper" => new RangeCriteria<decimal> { Gte = 1, Lt = 1 },
			_ => throw new ArgumentOutOfRangeException(nameof(scenario))
		};
		var result = await _validator.ValidateAsync(
			Query(new ProductSearchCriteria { Weight = new WeightSearchCriteria { Range = range } }),
			TestContext.Current.CancellationToken);
		result.IsValid.Should().BeFalse();
		result.Errors
			.Should()
			.Contain(error => error.PropertyName.StartsWith(
				"ProductCriteria.Weight.Range",
				StringComparison.Ordinal));
	}

	[Fact]
	public async Task PointAndOneSidedRanges_WorkWithoutATextQuery()
	{
		var criteria = new ProductSearchCriteria
		{
			Weight = new WeightSearchCriteria { Range = new RangeCriteria<decimal> { Gt = 0 } },
			Characteristics =
			[
				new CharacteristicSearchCriteria
				{
					CharacteristicId = 10, NumericRange = new RangeCriteria<decimal> { Gte = -20, Lte = -20 }
				}
			]
		};
		(await _validator.ValidateAsync(Query(criteria), TestContext.Current.CancellationToken)).IsValid
			.Should()
			.BeTrue();
	}

	[Theory]
	[InlineData("none")]
	[InlineData("valuesAndRange")]
	[InlineData("valuesAndExists")]
	[InlineData("rangeAndExists")]
	[InlineData("emptyValues")]
	[InlineData("blankValue")]
	[InlineData("invalidId")]
	public async Task InvalidCharacteristicCriterion_IsRejected(string scenario)
	{
		var criterion = scenario switch
		{
			"none" => new CharacteristicSearchCriteria { CharacteristicId = 10 },
			"valuesAndRange" => new CharacteristicSearchCriteria
			{
				CharacteristicId = 10, Values = ["10"], NumericRange = new RangeCriteria<decimal> { Gt = 0 }
			},
			"valuesAndExists" => new CharacteristicSearchCriteria
				{ CharacteristicId = 10, Values = ["10"], Exists = false },
			"rangeAndExists" => new CharacteristicSearchCriteria
			{
				CharacteristicId = 10, NumericRange = new RangeCriteria<decimal> { Gt = 0 }, Exists = true
			},
			"emptyValues" => new CharacteristicSearchCriteria { CharacteristicId = 10, Values = [] },
			"blankValue" => new CharacteristicSearchCriteria { CharacteristicId = 10, Values = [" "] },
			"invalidId" => new CharacteristicSearchCriteria { CharacteristicId = 0, Exists = true },
			_ => throw new ArgumentOutOfRangeException(nameof(scenario))
		};
		(await _validator.ValidateAsync(
				Query(new ProductSearchCriteria { Characteristics = [criterion] }),
				TestContext.Current.CancellationToken)).IsValid
			.Should()
			.BeFalse();
	}

	[Fact]
	public async Task RepeatedCharacteristicIds_AreRejected()
	{
		var criteria = new ProductSearchCriteria
		{
			Characteristics =
			[
				new CharacteristicSearchCriteria { CharacteristicId = 10, Exists = true },
				new CharacteristicSearchCriteria { CharacteristicId = 10, Values = ["10"] }
			]
		};
		(await _validator.ValidateAsync(Query(criteria), TestContext.Current.CancellationToken)).IsValid
			.Should()
			.BeFalse();
	}

	[Fact]
	public async Task ExcessiveCriterionAndValueCounts_AreRejected()
	{
		var criteria = new ProductSearchCriteria
		{
			Characteristics = Enumerable.Range(1, 101)
				.Select(id => new CharacteristicSearchCriteria { CharacteristicId = id, Exists = true })
				.ToArray()
		};
		(await _validator.ValidateAsync(Query(criteria), TestContext.Current.CancellationToken)).IsValid
			.Should()
			.BeFalse();
		criteria = new ProductSearchCriteria
		{
			Characteristics =
			[
				new CharacteristicSearchCriteria
				{
					CharacteristicId = 10,
					Values = Enumerable.Range(1, 101).Select(value => value.ToString()).ToArray()
				}
			]
		};
		(await _validator.ValidateAsync(Query(criteria), TestContext.Current.CancellationToken)).IsValid
			.Should()
			.BeFalse();
	}

	[Fact]
	public async Task EmptyDimensionsAndInvalidUnits_AreRejected()
	{
		foreach (var criteria in new ProductSearchCriteria[]
				{
					new() { Dimensions = new DimensionsSearchCriteria() },
					new()
					{
						Dimensions = new DimensionsSearchCriteria
							{ Unit = (DimensionUnit)999, Length = new RangeCriteria<decimal> { Gt = 0 } }
					},
					new()
					{
						Weight = new WeightSearchCriteria
							{ Unit = (WeightUnit)999, Range = new RangeCriteria<decimal> { Gt = 0 } }
					}
				})
			(await _validator.ValidateAsync(Query(criteria), TestContext.Current.CancellationToken)).IsValid
				.Should()
				.BeFalse();
	}

	[Fact]
	public async Task ProductCriteria_WithOnlyCandidateTarget_AreRejected()
	{
		var request =
			Query(
					new ProductSearchCriteria
					{
						Weight = new WeightSearchCriteria { Range = new RangeCriteria<decimal> { Gt = 0 } }
					})
				with
				{
					Targets = new HashSet<SearchTarget> { SearchTarget.CatalogueCandidates }
				};
		(await _validator.ValidateAsync(request, TestContext.Current.CancellationToken)).IsValid
			.Should()
			.BeFalse();
	}

	private static SearchInCatalogueQuery Query(ProductSearchCriteria criteria)
		=> new(
			null,
			new HashSet<SearchTarget> { SearchTarget.Products },
			new HashSet<SearchMatchType>(),
			new HashSet<SearchMatchType>(),
			[],
			new Pagination(0, 20),
			[],
			[],
			ProductCriteria: criteria);
}
