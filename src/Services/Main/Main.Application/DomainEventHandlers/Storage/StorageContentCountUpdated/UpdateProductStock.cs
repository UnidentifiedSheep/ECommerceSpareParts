using Application.Common.Abstractions;
using Application.Common.Extensions;
using Application.Common.Services.Events;
using Main.Application.Interfaces.Persistence;
using Main.Entities.DomainEvents.StorageContent;
using Main.Entities.Exceptions;

namespace Main.Application.DomainEventHandlers.Storage.StorageContentCountUpdated;

public class UpdateProductStock(
    IProductRepository productRepository
    ) : BatchableDomainEventHandler<StorageContentCountUpdatedDomainEvent>
{
    public override async Task Handle(Batch<StorageContentCountUpdatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var deltas = new Dictionary<int, int>();
        
        foreach (var item in notification.Items)
        {
            if (item.Delta == 0) continue;
            deltas[item.ProductId] = deltas.GetValueOrDefault(item.ProductId, 0) + item.Delta;
        }
        
        var products = await productRepository
            .EnsureExistsForUpdateAsync(
                deltas.Keys,
                notFound => new ProductNotFoundException(notFound),
                cancellationToken);

        foreach (var (id, product) in products)
            product.IncreaseStock(deltas[id]);
    }
}
