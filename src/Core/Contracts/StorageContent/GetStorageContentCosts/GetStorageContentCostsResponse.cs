using Contracts.Models.StorageContent;

namespace Contracts.StorageContent.GetStorageContentCosts;

public record GetStorageContentCostsResponse
{
    public List<StorageContentCost> StorageContentCosts { get; init; } = null!;
}