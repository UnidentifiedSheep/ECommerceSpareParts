using Search.Enums;
using Abstractions.Models;
using System.Runtime.Serialization;
using Application.Common.Models;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Search.Abstractions.Options;
using Search.Application.Interfaces.Product;
using Search.Application.Models.CatalogueSearch;
using Search.Application.Models;
using Search.Entities;
using Search.Persistence.Abstractions;
using Search.Persistence.Extensions;
using Search.Persistence.Interfaces;
using Search.Persistence.Models;
using Search.Persistence.Queries;

namespace Search.Persistence;

public class ProductRepository(
	IOptionsMonitor<OpenSearchOptions> options,
	IOpenSearchClient client,
	IIndexInitializer<Product> idxInitializer,
	IEnumerable<IProductSearchFilterProvider> filterProviders) : OpenSearchRepository<Product, int>(
		client,
		idxInitializer,
		() => options.CurrentValue.IndexOptions.Products,
		product => product.Id),
	IProductRepository
{
	private const string UpdateFieldsScript = """
		if (ctx._source[params.updatedAtField] == null || ctx._source[params.updatedAtField].compareTo(params.updatedAt) < 0) {
		    for (field in params.fieldNames) { ctx._source[field] = params.fields[field]; }
		    ctx._source[params.updatedAtField] = params.updatedAt;
		} else { ctx.op = 'none'; }
		""";

	public async Task ApplyUpdates(IReadOnlyCollection<ProductFieldUpdate> updates, CancellationToken cancellationToken = default)
	{
		if (updates.Count == 0) return;
		foreach (var update in updates)
			if (update.UpdatedAt.Kind != DateTimeKind.Utc)
				throw new ArgumentException("Snapshot timestamps must be UTC.", nameof(updates));
		var index = await GetIndex(cancellationToken);
		var response = await Client.BulkAsync(bulk =>
		{
			bulk.Index(index);
			foreach (var update in updates)
			{
				var fieldNames = update.GetType().GetProperties()
					.Where(property => !property.IsDefined(typeof(IgnoreDataMemberAttribute), true))
					.Select(property => Client.Infer.Field(new Field(property)))
					.ToArray();
				bulk.Update<ProductDocumentSeed, object>(operation => operation
					.Id(update.ProductId.ToString(System.Globalization.CultureInfo.InvariantCulture))
					.RetriesOnConflict(5)
					.ScriptedUpsert()
					.Upsert(new ProductDocumentSeed(update.ProductId))
					.Script(script => script.Source(UpdateFieldsScript).Params(parameters => parameters
						.Add("updatedAt", update.UpdatedAt.ToString("O", System.Globalization.CultureInfo.InvariantCulture))
						.Add("updatedAtField", Client.Infer.Field(new Field(update.GetUpdatedAtField())))
						.Add("fieldNames", fieldNames)
						.Add("fields", update))));
			}
			return bulk;
		}, cancellationToken);
		EnsureResponseSucceeded(response, "update product fields in");
		if (response.Errors)
			throw new InvalidOperationException(
				string.Join("; ",
					response.ItemsWithErrors.Select(x => $"{x.Id}: {x.Error?.Reason}")));
	}

	public async Task<SearchResult<Product>> Search(
		CatalogueSearchCriteria criteria,
		CancellationToken cancellationToken = default)
	{
		var index = await GetIndex(cancellationToken);
		var filters = new List<QueryContainer>
		{
			new QueryContainerDescriptor<Product>()
				.Term(term => term
					.Field(product => product.ReadinessStatus)
					.Value((int)ProductReadinessStatus.Ready))
		};
		if (criteria.ProductCriteria is { } productCriteria)
			filters.AddRange(filterProviders.SelectMany(provider => provider.Build(productCriteria)));
		Field normalizedSkuField = Client.Infer.Field(Infer.Field<Product>(product => product.NormalizedSku));
		Field nameField = Client.Infer.Field(Infer.Field<Product>(product => product.Name));
		var response = await Client.SearchAsync<Product>(
			search => search
				.Index(index)
				.From(GetFrom(criteria.Pagination))
				.Size(criteria.Pagination.Size)
				.TrackTotalHits()
				.SortBySearchRelevance(criteria.SortBy, product => product.Id)
				.Query(query => query.Bool(boolean => boolean
					.Filter(filters.ToArray())
					.Must(filter => CatalogueSearchQueryBuilder.Build(
					filter,
					criteria,
					normalizedSkuField,
					nameField,
					Infer.Field<Product>(product => product.ProducerId),
					productGroupIdField: Infer.Field<Product>(product => product.ProductGroupId)))))
				.AddCatalogueHighlights(
					criteria.IncludeHighlights,
					criteria.Query,
					Infer.Field<Product>(product => product.Sku),
					normalizedSkuField,
					nameField),
			cancellationToken);

		EnsureResponseSucceeded(response, "search in");
		return new SearchResult<Product>(
			response
				.Hits
				.Select(hit => new SearchHit<Product>(
					hit.Source,
					hit.Highlight.ToDictionary(
						pair => pair.Key,
						IReadOnlyCollection<string> (pair) => pair.Value)))
				.ToArray(),
			response.Total);
	}

	private static int GetFrom(Pagination pagination) => pagination.Page * pagination.Size;
}
