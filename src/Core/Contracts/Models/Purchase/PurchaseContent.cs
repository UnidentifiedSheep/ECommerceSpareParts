namespace Contracts.Models.Purchase;

public class PurchaseContent
{
    public required int Id { get; init; }

    public required Guid PurchaseId { get; init; }

    public required int ArticleId { get; init; }

    public required int Count { get; init; }

    public required decimal Price { get; init; }

    public required decimal TotalSum { get; init; }

    public string? Comment { get; init; }

    public int? StorageContentId { get; init; }
}