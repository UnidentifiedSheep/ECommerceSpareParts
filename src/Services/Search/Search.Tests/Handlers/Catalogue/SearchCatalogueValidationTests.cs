using Abstractions.Models;
using FluentAssertions;
using Search.Application.Handlers.Catalogue.SearchCatalogue;
using Search.Enums;

namespace Search.Tests.Handlers.Catalogue;

public sealed class SearchCatalogueValidationTests
{
    private readonly SearchCatalogueValidation _validator = new();

    [Fact]
    public async Task Validate_WhenTargetsAreEmpty_ShouldFail()
    {
        var result = await _validator.ValidateAsync(
            CreateQuery(targets: new HashSet<SearchTarget>()));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error =>
            error.PropertyName == nameof(SearchCatalogueQuery.Targets));
    }

    [Fact]
    public async Task Validate_WhenTextQueryHasNoModes_ShouldFail()
    {
        var result = await _validator.ValidateAsync(
            CreateQuery(
                skuModes: new HashSet<SearchMatchType>(),
                nameModes: new HashSet<SearchMatchType>()));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenQueryIsEmptyAndModesAreEmpty_ShouldSucceed()
    {
        var result = await _validator.ValidateAsync(
            CreateQuery(
                query: null,
                skuModes: new HashSet<SearchMatchType>(),
                nameModes: new HashSet<SearchMatchType>()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenShortQueryHasOnlyFuzzyMode_ShouldFail()
    {
        var fuzzyModes = new HashSet<SearchMatchType> { SearchMatchType.Fuzzy };

        var result = await _validator.ValidateAsync(
            CreateQuery(
                query: "abc",
                skuModes: fuzzyModes,
                nameModes: fuzzyModes));

        result.IsValid.Should().BeFalse();
    }

    private static SearchCatalogueQuery CreateQuery(
        string? query = "bosch",
        IReadOnlySet<SearchTarget>? targets = null,
        IReadOnlySet<SearchMatchType>? skuModes = null,
        IReadOnlySet<SearchMatchType>? nameModes = null)
    {
        return new SearchCatalogueQuery(
            query,
            targets ?? new HashSet<SearchTarget> { SearchTarget.Products },
            skuModes ?? new HashSet<SearchMatchType> { SearchMatchType.Exact },
            nameModes ?? new HashSet<SearchMatchType> { SearchMatchType.StartsWith },
            [],
            new Pagination(0, 20),
            [],
            []);
    }
}
