using Abstractions.Models;
using FluentAssertions;
using Search.Application.Handlers.Search.Catalogue;
using Search.Enums;

namespace Search.Tests.Handlers.Catalogue;

public sealed class SearchInCatalogueValidationTests
{
	private readonly SearchInCatalogueValidation _validator = new();

	[Fact]
	public async Task Validate_WhenTargetsAreEmpty_ShouldFail()
	{
		var result = await _validator.ValidateAsync(
			CreateQuery(targets: new HashSet<SearchTarget>()),
			TestContext.Current.CancellationToken);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(error => error.PropertyName == nameof(SearchInCatalogueQuery.Targets));
	}

	[Fact]
	public async Task Validate_WhenTextQueryHasNoModes_ShouldFail()
	{
		var result = await _validator.ValidateAsync(
			CreateQuery(skuModes: new HashSet<SearchMatchType>(), nameModes: new HashSet<SearchMatchType>()),
			TestContext.Current.CancellationToken);

		result.IsValid.Should().BeFalse();
	}

	[Fact]
	public async Task Validate_WhenQueryIsEmptyAndModesAreEmpty_ShouldSucceed()
	{
		var result = await _validator.ValidateAsync(
			CreateQuery(
				null,
				skuModes: new HashSet<SearchMatchType>(),
				nameModes: new HashSet<SearchMatchType>()),
			TestContext.Current.CancellationToken);

		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public async Task Validate_WhenShortQueryHasOnlyFuzzyMode_ShouldFail()
	{
		var fuzzyModes = new HashSet<SearchMatchType>
		{
			SearchMatchType.Fuzzy
		};

		var result = await _validator.ValidateAsync(
			CreateQuery(
				"abc",
				skuModes: fuzzyModes,
				nameModes: fuzzyModes),
			TestContext.Current.CancellationToken);

		result.IsValid.Should().BeFalse();
	}

	[Fact]
	public async Task Validate_WhenCandidateMappingStatusIsInvalid_ShouldFail()
	{
		var result = await _validator.ValidateAsync(
			CreateQuery() with
			{
				CandidateMappingStatus = (CandidateMappingStatus)999
			},
			TestContext.Current.CancellationToken);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(
			error => error.PropertyName == nameof(SearchInCatalogueQuery.CandidateMappingStatus));
	}

	private static SearchInCatalogueQuery CreateQuery(
		string? query = "bosch",
		IReadOnlySet<SearchTarget>? targets = null,
		IReadOnlySet<SearchMatchType>? skuModes = null,
		IReadOnlySet<SearchMatchType>? nameModes = null)
	{
		return new SearchInCatalogueQuery(
			query,
			targets ?? new HashSet<SearchTarget>
			{
				SearchTarget.Products
			},
			skuModes ?? new HashSet<SearchMatchType>
			{
				SearchMatchType.Exact
			},
			nameModes ?? new HashSet<SearchMatchType>
			{
				SearchMatchType.StartsWith
			},
			[],
			new Pagination(0, 20),
			[],
			[]);
	}
}
