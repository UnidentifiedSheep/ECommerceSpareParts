namespace Analytics.Abstractions.Dtos.PurchaseFact;

public record PurchaseContentUpsertDto
{
    public required int Id { get; init; }

    public required int ArticleId { get; init; }

    public required decimal Price { get; init; }

    public required int Count { get; init; }
}