namespace Search.Application.Models.CatalogueSearch;

public sealed record SearchResult<TDocument>(
    IReadOnlyCollection<TDocument> Items,
    long Total);
