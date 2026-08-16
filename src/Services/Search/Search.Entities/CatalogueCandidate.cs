namespace Search.Entities;

public sealed class CatalogueCandidate
{
    public required Guid Id { get; init; }

    public required string Sku { get; init; }

    public required string NormalizedSku { get; init; }

    public required int ProducerId { get; init; }

    public required int? MappedProductId { get; init; }

    public required IReadOnlyCollection<string> Names { get; init; }
}
