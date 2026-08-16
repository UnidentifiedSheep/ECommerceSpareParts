using Search.Enums;

namespace Search.Application.Models.CatalogueSearch;

public static class CatalogueSearchDefaults
{
    public static readonly IReadOnlySet<SearchMatchType> SkuModes = new HashSet<SearchMatchType>
    {
        SearchMatchType.Exact,
        SearchMatchType.StartsWith,
        SearchMatchType.Contains
    };

    public static readonly IReadOnlySet<SearchMatchType> NameModes = new HashSet<SearchMatchType>
    {
        SearchMatchType.Exact,
        SearchMatchType.StartsWith,
        SearchMatchType.Fuzzy
    };

    public static readonly IReadOnlySet<SearchTarget> Targets = new HashSet<SearchTarget>
    {
        SearchTarget.Products,
        SearchTarget.CatalogueCandidates
    };
}
