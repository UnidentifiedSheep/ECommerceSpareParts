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
			"empty" => new RangeCriteria<int>(),
			"twoLowerBounds" => new() { Gt = 1, Gte = 1 },
			"twoUpperBounds" => new() { Lt = 2, Lte = 2 },
			"reversed" => new() { Gte = 2, Lte = 1 },
			"emptyExclusiveLower" => new() { Gt = 1, Lte = 1 },
			"emptyExclusiveUpper" => new() { Gte = 1, Lt = 1 },
			_ => throw new ArgumentOutOfRangeException(nameof(scenario))
		};
		var result = await _validator.ValidateAsync(Query(new() { Stock = range }), TestContext.Current.CancellationToken);
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(error => error.PropertyName.StartsWith("ProductCriteria.Stock", StringComparison.Ordinal));
	}

	[Fact]
	public async Task PointAndOneSidedRanges_WorkWithoutATextQuery()
	{
		var criteria = new ProductSearchCriteria
		{
			Stock = new() { Gt = 0 },
			Characteristics = [new() { CharacteristicId = 10, NumericRange = new() { Gte = -20, Lte = -20 } }]
		};
		(await _validator.ValidateAsync(Query(criteria), TestContext.Current.CancellationToken)).IsValid.Should().BeTrue();
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
			"valuesAndRange" => new() { CharacteristicId = 10, Values = ["10"], NumericRange = new() { Gt = 0 } },
			"valuesAndExists" => new() { CharacteristicId = 10, Values = ["10"], Exists = false },
			"rangeAndExists" => new() { CharacteristicId = 10, NumericRange = new() { Gt = 0 }, Exists = true },
			"emptyValues" => new() { CharacteristicId = 10, Values = [] },
			"blankValue" => new() { CharacteristicId = 10, Values = [" "] },
			"invalidId" => new() { CharacteristicId = 0, Exists = true },
			_ => throw new ArgumentOutOfRangeException(nameof(scenario))
		};
		(await _validator.ValidateAsync(Query(new() { Characteristics = [criterion] }), TestContext.Current.CancellationToken)).IsValid.Should().BeFalse();
	}

	[Fact]
	public async Task RepeatedCharacteristicIds_AreRejected()
	{
		var criteria = new ProductSearchCriteria
		{
			Characteristics = [new() { CharacteristicId = 10, Exists = true }, new() { CharacteristicId = 10, Values = ["10"] }]
		};
		(await _validator.ValidateAsync(Query(criteria), TestContext.Current.CancellationToken)).IsValid.Should().BeFalse();
	}

	[Fact]
	public async Task ExcessiveCriterionAndValueCounts_AreRejected()
	{
		var criteria = new ProductSearchCriteria
		{
			Characteristics = Enumerable.Range(1, 101).Select(id => new CharacteristicSearchCriteria { CharacteristicId = id, Exists = true }).ToArray()
		};
		(await _validator.ValidateAsync(Query(criteria), TestContext.Current.CancellationToken)).IsValid.Should().BeFalse();
		criteria = new() { Characteristics = [new() { CharacteristicId = 10, Values = Enumerable.Range(1, 101).Select(value => value.ToString()).ToArray() }] };
		(await _validator.ValidateAsync(Query(criteria), TestContext.Current.CancellationToken)).IsValid.Should().BeFalse();
	}

	[Fact]
	public async Task EmptyDimensionsAndInvalidUnits_AreRejected()
	{
		foreach (var criteria in new ProductSearchCriteria[]
		{
			new() { Dimensions = new() },
			new() { Dimensions = new() { Unit = (DimensionUnit)999, Length = new() { Gt = 0 } } },
			new() { Weight = new() { Unit = (WeightUnit)999, Range = new() { Gt = 0 } } }
		})
			(await _validator.ValidateAsync(Query(criteria), TestContext.Current.CancellationToken)).IsValid.Should().BeFalse();
	}

	[Fact]
	public async Task ProductCriteria_WithOnlyCandidateTarget_AreRejected()
	{
		var request = Query(new() { Stock = new() { Gt = 0 } }) with { Targets = new HashSet<SearchTarget> { SearchTarget.CatalogueCandidates } };
		(await _validator.ValidateAsync(request, TestContext.Current.CancellationToken)).IsValid.Should().BeFalse();
	}

	private static SearchInCatalogueQuery Query(ProductSearchCriteria criteria) =>
		new(null, new HashSet<SearchTarget> { SearchTarget.Products }, new HashSet<SearchMatchType>(),
			new HashSet<SearchMatchType>(), [], new Pagination(0, 20), [], [], ProductCriteria: criteria);
}
