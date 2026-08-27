using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using MediatR;
using Search.Api.GraphQl.Types;
using Search.Api.GraphQl.Types.CatalogueSearch;
using Search.Api.GraphQl.Types.Highlights;
using Search.Application.Handlers.Catalogue.SearchCatalogue;

namespace Search.Api.GraphQl.Queries;

public sealed class CatalogueQueries
{
    [GraphQLName("search")]
    [RequireAllPermissions(
        PermissionCodes.ARTICLES_GET_MAIN, 
        PermissionCodes.CATALOGUE_CANDIDATES_REVIEW)]
    public async Task<GqlCatalogueSearchResult> SearchAsync(
        ISender sender,
        GqlCatalogueSearchInput input,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new SearchCatalogueQuery(
                input.Query,
                input.Targets.ToHashSet(),
                input.SkuModes.ToHashSet(),
                input.NameModes.ToHashSet(),
                input.ProducerIds ?? [],
                input.Pagination,
                input.ProductSortBy?.Select(x => x.ToSortExpression()).ToArray() ?? [],
                input.CatalogueCandidateSortBy?.Select(x => x.ToSortExpression()).ToArray() ?? [],
                input.IncludeHighlights ?? false),
            ct);


        return new GqlCatalogueSearchResult
        {
            Products = new GqlSearchCatalogueSection<GqlProduct>
            {
                Total = result.Products.Total,
                Items = result.Products
                    .Items
                    .Select(x => new GqlSearchCatalogueSectionItem<GqlProduct>
                    {
                        Item = new GqlProduct(x.Id),
                        Highlights = GqlHighlights.From(x.Highlights)
                    })
                    .ToArray()
            },
            Candidates = new GqlSearchCatalogueSection<GqlCatalogueCandidate>
            {
                Total = result.CatalogueCandidates.Total,
                Items = result.CatalogueCandidates
                    .Items
                    .Select(x => new GqlSearchCatalogueSectionItem<GqlCatalogueCandidate>
                    {
                        Item = new GqlCatalogueCandidate(x.Id),
                        Highlights = GqlHighlights.From(x.Highlights)
                    })
                    .ToArray()
            },
        };
    }
}
