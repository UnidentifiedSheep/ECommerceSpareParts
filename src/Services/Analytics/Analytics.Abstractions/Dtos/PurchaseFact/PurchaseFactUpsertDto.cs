namespace Analytics.Abstractions.Dtos.PurchaseFact;

public record PurchaseFactUpsertDto
{
    public required string Id { get; init; }

    public required int CurrencyId { get; init; }

    public required Guid SupplierId { get; init; }

    public required IReadOnlyList<PurchaseContentUpsertDto> Content { get; init; }
}