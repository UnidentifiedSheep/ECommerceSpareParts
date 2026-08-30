using Abstractions.Models;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Search.Abstractions.Options;
using Search.Application.Interfaces.Producer;
using Search.Entities;
using Search.Persistence.Abstractions;
using Search.Persistence.Interfaces;

namespace Search.Persistence;

public class ProducerRepository(
	IOptionsMonitor<OpenSearchOptions> options,
	IOpenSearchClient client,
	IIndexInitializer<Producer> idxInitializer) : OpenSearchRepository<Producer, int>(
		client,
		idxInitializer,
		() => options.CurrentValue.IndexOptions.Producers,
		producer => producer.Id),
	IProducerRepository
{
	private static readonly Pagination DefaultPagination = new(0, 20);

	public async Task<IReadOnlyCollection<Producer>> Search(
		string? query,
		Pagination? pagination = null,
		CancellationToken token = default)
	{
		var page = pagination ?? DefaultPagination;
		var idx = await GetIndex(token);
		var searchQuery = query?.Trim() ?? "";

		var response = await Client.SearchAsync<Producer>(
			s => s
				.Index(idx)
				.From(GetFrom(page))
				.Size(page.Size)
				.Query(q => q.Bool(b => b
					.Should(
						sh => sh.Match(m => m.Field(p => p.Name).Query(searchQuery).Boost(2)),
						sh => sh.Match(m => m.Field("aliases.alias").Query(searchQuery)))
					.MinimumShouldMatch(1))),
			token);

		return response.Documents;
	}

	private static int GetFrom(Pagination pagination) => pagination.Page * pagination.Size;
}
