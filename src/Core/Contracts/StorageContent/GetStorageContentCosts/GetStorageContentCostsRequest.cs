namespace Contracts.StorageContent.GetStorageContentCosts;

public record GetStorageContentCostsRequest
{
    public IEnumerable<int> ArticleIds { get; init; } = null!;
    public bool OnlyPositiveQty { get; init; } = true;
}