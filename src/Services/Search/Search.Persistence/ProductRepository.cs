using Abstractions.Models;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Search.Abstractions.Options;
using Search.Application.Interfaces.Product;
using Search.Application.Models.CatalogueSearch;
using Search.Entities;
using Search.Persistence.Abstractions;
using Search.Persistence.Extensions;
using Search.Persistence.Interfaces;
using Search.Persistence.Queries;

namespace Search.Persistence;

public class ProductRepository(
	IOptionsMonitor<OpenSearchOptions> options,
	IOpenSearchClient client,
	IIndexInitializer<Product> idxInitializer) : OpenSearchRepository<Product, int>(
		client,
		idxInitializer,
		() => options.CurrentValue.IndexOptions.Products,
		product => product.Id),
	IProductRepository
{
	public async Task<SearchResult<Product>> Search(
		CatalogueSearchCriteria criteria,
		CancellationToken cancellationToken = default)
	{
		var index = await GetIndex(cancellationToken);
		var response = await Client.SearchAsync<Product>(
			search => search
				.Index(index)
				.From(GetFrom(criteria.Pagination))
				.Size(criteria.Pagination.Size)
				.TrackTotalHits()
				.SortBySearchRelevance(criteria.SortBy, product => product.Id)
				.Query(query => CatalogueSearchQueryBuilder.Build(
					query,
					criteria,
					new Field("normalizedSku"),
					new Field("name"),
					new Field("producerId")))
				.AddCatalogueHighlights(
					criteria.IncludeHighlights,
					criteria.Query,
					new Field("sku"),
					new Field("normalizedSku"),
					new Field("name")),
			cancellationToken);

		EnsureResponseSucceeded(response, "search in");
		return new SearchResult<Product>(
			response
				.Hits
				.Select(hit => new SearchHit<Product>(
					hit.Source,
					hit.Highlight.ToDictionary(
						pair => pair.Key,
						pair => (IReadOnlyCollection<string>)pair.Value)))
				.ToArray(),
			response.Total);
	}

	private static int GetFrom(Pagination pagination) => pagination.Page * pagination.Size;
}
