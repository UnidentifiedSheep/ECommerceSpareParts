using Abstractions.Models;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Search.Application.Dtos.CatalogueCandidates;
using Search.Application.Dtos.Products;
using Search.Application.Interfaces.CatalogueCandidate;
using Search.Application.Interfaces.Product;
using Search.Application.Models.CatalogueSearch;
using Search.Enums;
using CatalogueCandidateDocument = Search.Entities.CatalogueCandidate;
using ProductDocument = Search.Entities.Product;

namespace Search.Application.Handlers.Search.Catalogue;

public sealed record CatalogueQuery(
	string? Query,
	IReadOnlySet<SearchTarget> Targets,
	IReadOnlySet<SearchMatchType> SkuModes,
	IReadOnlySet<SearchMatchType> NameModes,
	IReadOnlyCollection<int> ProducerIds,
	Pagination Pagination,
	string[] ProductSortBy,
	string[] CatalogueCandidateSortBy,
	CandidateMappingStatus CandidateMappingStatus = CandidateMappingStatus.Unmapped,
	bool IncludeHighlights = false) : IQuery<CatalogueResult>;

public sealed record CatalogueSection<T>(IReadOnlyCollection<T> Items, long Total);

public sealed record CatalogueResult(
	CatalogueSection<ProductDto> Products,
	CatalogueSection<CatalogueCandidateDto> CatalogueCandidates);

public sealed class CatalogueHandler(
	IProductRepository productRepository,
	ICatalogueCandidateRepository candidateRepository,
	IProjectionProvider<ProductDocument, ProductDto> productProjection,
	IProjectionProvider<CatalogueCandidateDocument, CatalogueCandidateDto> candidateProjection)
	: IQueryHandler<CatalogueQuery, CatalogueResult>
{
	public async Task<CatalogueResult> Handle(
		CatalogueQuery request,
		CancellationToken cancellationToken)
	{
		var commonCriteria = new CatalogueSearchCriteria
		{
			Query = request.Query?.Trim() ?? string.Empty,
			SkuModes = request.SkuModes,
			NameModes = request.NameModes,
			ProducerIds = request.ProducerIds.Distinct().ToArray(),
			CandidateMappingStatus = request.CandidateMappingStatus,
			Pagination = request.Pagination,
			SortBy = [],
			IncludeHighlights = request.IncludeHighlights
		};

		var productTask = request.Targets.Contains(SearchTarget.Products)
			? productRepository.Search(
				commonCriteria with
				{
					SortBy = request.ProductSortBy
				},
				cancellationToken)
			: Task.FromResult(new SearchResult<ProductDocument>([], 0));
		var candidateTask = request.Targets.Contains(SearchTarget.CatalogueCandidates)
			? candidateRepository.Search(
				commonCriteria with
				{
					SortBy = request.CatalogueCandidateSortBy
				},
				cancellationToken)
			: Task.FromResult(new SearchResult<CatalogueCandidateDocument>([], 0));

		await Task.WhenAll(productTask, candidateTask);
		var products = await productTask;
		var candidates = await candidateTask;

		return new CatalogueResult(
			new CatalogueSection<ProductDto>(
				products
					.Hits
					.Select(hit => productProjection.ProjectionFunc(hit.Document) with
					{
						Highlights = ToHighlights(hit.Highlights)
					})
					.ToArray(),
				products.Total),
			new CatalogueSection<CatalogueCandidateDto>(
				candidates
					.Hits
					.Select(hit => candidateProjection.ProjectionFunc(hit.Document) with
					{
						Highlights = ToHighlights(hit.Highlights)
					})
					.ToArray(),
				candidates.Total));
	}

	private static IReadOnlyDictionary<string, IReadOnlyCollection<string>>? ToHighlights(
		IReadOnlyDictionary<string, IReadOnlyCollection<string>> highlights) =>
		highlights.Count == 0 ? null : highlights;
}
