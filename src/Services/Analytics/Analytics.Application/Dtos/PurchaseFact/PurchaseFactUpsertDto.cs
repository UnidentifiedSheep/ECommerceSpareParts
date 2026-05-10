namespace Analytics.Application.Dtos.PurchaseFact;

public record PurchaseFactUpsertDto
{
    public required Guid Id { get; init; }

    public required int CurrencyId { get; init; }

    public required Guid SupplierId { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required DateTime LastUpdatedAt { get; init; }

    public required IReadOnlyList<PurchaseContentUpsertDto> Content { get; init; }
}