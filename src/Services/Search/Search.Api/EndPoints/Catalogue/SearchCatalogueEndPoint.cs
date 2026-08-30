using System.Text.Json.Serialization;
using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Search.Application.Dtos.CatalogueCandidates;
using Search.Application.Dtos.Products;
using Search.Application.Handlers.Catalogue.SearchCatalogue;
using Search.Application.Models.CatalogueSearch;
using Search.Enums;

namespace Search.Api.EndPoints.Catalogue;

public sealed record CatalogueSearchFieldsRequest
{
	[JsonPropertyName("sku")]
	public IReadOnlyCollection<SearchMatchType>? Sku { get; init; }

	[JsonPropertyName("name")]
	public IReadOnlyCollection<SearchMatchType>? Name { get; init; }
}

public sealed record SearchCatalogueRequest
{
	[JsonPropertyName("query")]
	public string? Query { get; init; }

	[JsonPropertyName("targets")]
	public IReadOnlyCollection<SearchTarget>? Targets { get; init; }

	[JsonPropertyName("fields")]
	public CatalogueSearchFieldsRequest? Fields { get; init; }

	[JsonPropertyName("producerIds")]
	public IReadOnlyCollection<int> ProducerIds { get; init; } = [];

	[JsonPropertyName("page")]
	public int Page { get; init; }

	[JsonPropertyName("size")]
	public int Size { get; init; } = 20;

	[JsonPropertyName("sortBy")]
	public CatalogueSearchSortRequest? SortBy { get; init; }

	[JsonPropertyName("includeHighlights")]
	public bool IncludeHighlights { get; init; }
}

public sealed record CatalogueSearchSortRequest
{
	[JsonPropertyName("products")]
	public string[] Products { get; init; } = [];

	[JsonPropertyName("catalogueCandidates")]
	public string[] CatalogueCandidates { get; init; } = [];
}

public sealed record SearchCatalogueResponse
{
	[JsonPropertyName("products")]
	public required SearchCatalogueSection<ProductDto> Products { get; init; }

	[JsonPropertyName("catalogueCandidates")]
	public required SearchCatalogueSection<CatalogueCandidateDto> CatalogueCandidates { get; init; }
}

public sealed class SearchCatalogueEndPoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app
			.MapPost(
				"/catalogue/search",
				async (
					ISender sender, [FromBody] SearchCatalogueRequest request,
					CancellationToken cancellationToken) =>
				{
					var result = await sender.Send(
						new SearchCatalogueQuery(
							request.Query,
							(request.Targets ?? CatalogueSearchDefaults.Targets).ToHashSet(),
							(request.Fields?.Sku ?? CatalogueSearchDefaults.SkuModes).ToHashSet(),
							(request.Fields?.Name ?? CatalogueSearchDefaults.NameModes).ToHashSet(),
							request.ProducerIds,
							new Pagination(request.Page, request.Size),
							request.SortBy?.Products ?? [],
							request.SortBy?.CatalogueCandidates ?? [],
							request.IncludeHighlights),
						cancellationToken);

					return Results.Ok(
						new SearchCatalogueResponse
						{
							Products = result.Products, CatalogueCandidates = result.CatalogueCandidates
						});
				})
			.WithTags("Catalogue")
			.WithName("SearchCatalogue")
			.WithSummary("Search products and unresolved catalogue candidates")
			.Produces<SearchCatalogueResponse>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.RequireAllPermissions(
				PermissionCodes.ARTICLES_GET_MAIN,
				PermissionCodes.CATALOGUE_CANDIDATES_REVIEW);
	}
}
