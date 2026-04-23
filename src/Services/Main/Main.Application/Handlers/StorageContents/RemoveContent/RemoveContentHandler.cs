using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Articles;
using Contracts.StorageContent;
using Main.Abstractions.Exceptions.Storages;
using Main.Abstractions.Models;
using Main.Application.Extensions;
using Main.Application.Interfaces.Repositories;
using Main.Entities.Event;
using Main.Entities.Product;
using Main.Entities.Storage;
using Main.Enums;

namespace Main.Application.Handlers.StorageContents.RemoveContent;

[AutoSave]
[Transactional(IsolationLevel.Serializable, 20, 2)]
public record RemoveContentCommand(
    Dictionary<int, int> Content,
    string? StorageName,
    bool TakeFromOtherStorages,
    StorageMovementType MovementType) : ICommand<RemoveContentResult>;

public record RemoveContentResult(IEnumerable<PrevAndNewValue<StorageContent>> Changes);

public class RemoveContentHandler(
    IStorageContentRepository contentRepository,
    IProductRepository productsRepository,
    IIntegrationEventScope integrationEventScope,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveContentCommand, RemoveContentResult>
{
    public async Task<RemoveContentResult> Handle(RemoveContentCommand request, CancellationToken cancellationToken)
    {
        var content = request.Content;
        var takeFromOtherStorages = request.TakeFromOtherStorages;
        var storageName = request.StorageName;
        var productIds = content.Keys;
        
        var products = await productsRepository
            .EnsureProductsExistsForUpdateAsync(productIds, cancellationToken);

        var movements = new List<Event>();
        var result = new List<PrevAndNewValue<StorageContent>>();

        foreach (var (productId, count) in content)
        {
            Product product = products[productId];
            List<StorageContent> storageContents = [];

            var availableCount = 0;
            if (!string.IsNullOrWhiteSpace(storageName))
                await foreach (var dbItem in contentRepository
                                   .GetStorageContentsForUpdateAsync(productId, storageName)
                                   .WithCancellation(cancellationToken))
                {
                    storageContents.Add(dbItem);
                    availableCount += dbItem.Count;
                    if (availableCount >= count) break;
                }

            if (takeFromOtherStorages && availableCount < count)
                await foreach (var dbItem in contentRepository
                                   .GetStorageContentsForUpdateAsync(productId, null, null,
                                       string.IsNullOrWhiteSpace(storageName) ? null : [storageName])
                                   .WithCancellation(cancellationToken))
                {
                    storageContents.Add(dbItem);
                    availableCount += dbItem.Count;
                    if (availableCount >= count) break;
                }

            if (availableCount < count) throw new NotEnoughCountOnStorageException(productId, availableCount, count);
            var counter = count;

            foreach (var item in storageContents)
            {
                StorageContent prevValue = StorageContent.CopyFrom(item);
                
                int temp = Math.Min(counter, item.Count);
                item.IncreaseCount(-temp);
                counter -= temp;
                
                StorageContent newValue = StorageContent.CopyFrom(item);
                
                Event movementEvent = StorageMovementEvent.Create(item, request.MovementType);
                movements.Add(movementEvent);

                result.Add(new PrevAndNewValue<StorageContent>(prevValue, newValue));
                if (counter <= 0) break;
            }

            product.IncreaseStock(-count);
        }

        await unitOfWork.AddRangeAsync(movements, cancellationToken);

        foreach (var id in productIds)
        {
            integrationEventScope.Add(new StorageContentUpdatedEvent
            {
                ProductId = id
            });
            
            integrationEventScope.Add(new ProductUpdatedEvent
            {
                Id = id
            });
        }

        return new RemoveContentResult(result);
    }
}