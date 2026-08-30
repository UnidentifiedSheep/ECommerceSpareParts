namespace Search.Application.Models.CatalogueSearch;

public sealed record SearchHit<TDocument>(
	TDocument Document,
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> Highlights);

public sealed record SearchResult<TDocument>(IReadOnlyCollection<SearchHit<TDocument>> Hits, long Total);
