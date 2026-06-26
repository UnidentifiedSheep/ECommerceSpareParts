using Application.Common.Interfaces;
using Contracts.Products;
using Main.Application.Interfaces.Services.Storage;

namespace Main.Application.Services.Storage;

public sealed class StorageContentChangeNotifier(
    IIntegrationEventScope integrationEventScope) : IStorageContentChangeNotifier
{
    public void NotifyChanged(IEnumerable<int> productIds)
    {
        foreach (var productId in productIds.Distinct())
            integrationEventScope.Add(new ProductUpdatedEvent { Id = productId });
    }
}