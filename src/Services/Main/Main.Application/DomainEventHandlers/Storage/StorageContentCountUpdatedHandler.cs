using Application.Common.Abstractions;
using Application.Common.Extensions;
using Application.Common.Services.Events;
using Main.Application.Interfaces.Persistence;
using Main.Entities.DomainEvents.StorageContent;
using Main.Entities.Exceptions;

namespace Main.Application.DomainEventHandlers.Storage;

public class StorageContentCountUpdatedHandler(
    IProductRepository productRepository
    ) : BatchableDomainEventHandler<StorageContentCountUpdatedDomainEvent>
{
    public override async Task Handle(Batch<StorageContentCountUpdatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var deltas = notification.Items
            .GroupBy(x => x.ProductId, x => x.Delta)
            .Select(x => (x.Key, x.Sum()))
            .Where(x => x.Item2 != 0)
            .ToDictionary(x => x.Key, x => x.Item2);
        
        var products = await productRepository
            .EnsureExistsForUpdateAsync(
                deltas.Keys,
                notFound => new ProductNotFoundException(notFound),
                cancellationToken);

        foreach (var (id, product) in products)
            product.IncreaseStock(deltas[id]);
    }
}
