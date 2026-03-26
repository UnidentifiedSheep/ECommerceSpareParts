using Enums;

namespace Contracts.Models.Purchase;

public class Purchase
{
    public required string Id { get; init; }

    public required Guid CreatedUserId { get; init; }

    public required Guid SupplierId { get; init; }

    public string? Comment { get; init; }

    public required DateTime PurchaseDatetime { get; init; }

    public required DateTime CreationDatetime { get; init; }

    public DateTime? LastUpdatedAt { get; init; }

    public required int CurrencyId { get; init; }

    public required Guid TransactionId { get; init; }

    public required string Storage { get; init; }

    public required PurchaseState State { get; init; }
    public required List<PurchaseContent> PurchaseContents { get; init; }
}