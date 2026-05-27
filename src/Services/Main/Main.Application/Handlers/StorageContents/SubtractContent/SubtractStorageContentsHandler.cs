using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Contracts.Products;
using Contracts.StorageContent;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Main.Entities.Storage;
using Main.Enums;
using Event = Main.Entities.Event.Event;

namespace Main.Application.Handlers.StorageContents.SubtractContent;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record SubtractStorageContentsCommand(
    IReadOnlyList<SubtractStorageContentItem> Items,
    StorageMovementType MovementType) : ICommand<SubtractStorageContentsResult>
{
    public SubtractStorageContentsCommand(
        int storageContentId,
        int count,
        StorageMovementType movementType)
        : this([new SubtractStorageContentItem(storageContentId, count)], movementType)
    {
    }
}

public record SubtractStorageContentsResult(IReadOnlyList<SubtractedStorageContent> Contents);

public record SubtractStorageContentItem(int StorageContentId, int Count);

public record SubtractedStorageContent(int StorageContentId, int Count);

public class SubtractStorageContentsHandler(
    IStorageContentRepository storageContentRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope)
    : ICommandHandler<SubtractStorageContentsCommand, SubtractStorageContentsResult>
{
    public async Task<SubtractStorageContentsResult> Handle(
        SubtractStorageContentsCommand request,
        CancellationToken cancellationToken)
    {
        var items = request.Items.ToList();
        var firstContentIds = items
            .Select(x => x.StorageContentId)
            .Distinct()
            .ToArray();

        var firstContents = await storageContentRepository
            .EnsureExistsForUpdateAsync(
                firstContentIds, 
                ids => new StorageContentNotFoundException(ids[0]), 
                cancellationToken);

        var productIds = firstContents.Values
            .Select(x => x.ProductId)
            .Distinct()
            .ToArray();

        var products = await productRepository.EnsureExistsForUpdateAsync(
            productIds,
            ids => new ProductNotFoundException(ids),
            cancellationToken);

        var affected = new List<SubtractedStorageContent>();
        var events = new List<Event>();
        var affectedProductIds = new HashSet<int>();

        foreach (var item in items)
            await SubtractItem(
                item,
                firstContents[item.StorageContentId],
                products,
                affected,
                events,
                affectedProductIds,
                request.MovementType,
                cancellationToken);

        await unitOfWork.AddRangeAsync(events, cancellationToken);

        foreach (var productId in affectedProductIds)
        {
            integrationEventScope.Add(new ProductUpdatedEvent
            {
                Id = productId
            });

            integrationEventScope.Add(new StorageContentUpdatedEvent
            {
                ProductId = productId
            });
        }

        return new SubtractStorageContentsResult(affected);
    }

    private async Task SubtractItem(
        SubtractStorageContentItem item,
        StorageContent firstContent,
        IReadOnlyDictionary<int, Product> products,
        ICollection<SubtractedStorageContent> affected,
        ICollection<Event> events,
        ISet<int> affectedProductIds,
        StorageMovementType movementType,
        CancellationToken cancellationToken)
    {
        var remaining = item.Count;

        Subtract(firstContent, ref remaining, affected, events, movementType);

        if (remaining > 0)
            await foreach (var content in storageContentRepository
                               .GetStorageContentsForUpdateAsync(
                                   firstContent.ProductId,
                                   firstContent.StorageName)
                               .WithCancellation(cancellationToken))
            {
                if (content.Id == firstContent.Id) continue;

                Subtract(content, ref remaining, affected, events, movementType);
                if (remaining == 0) break;
            }

        if (remaining > 0)
        {
            var availableCount = item.Count - remaining;
            throw new NotEnoughCountOnStorageException(
                firstContent.ProductId,
                availableCount,
                item.Count);
        }

        products[firstContent.ProductId].IncreaseStock(-item.Count);
        affectedProductIds.Add(firstContent.ProductId);
    }

    private static void Subtract(
        StorageContent content,
        ref int remaining,
        ICollection<SubtractedStorageContent> affected,
        ICollection<Event> events,
        StorageMovementType movementType)
    {
        if (remaining == 0 || content.Count == 0) return;

        var countToSubtract = Math.Min(content.Count, remaining);
        events.Add(CreateStorageMovementEvent(content, countToSubtract, movementType));
        content.IncreaseCount(-countToSubtract);
        affected.Add(new SubtractedStorageContent(content.Id, countToSubtract));
        remaining -= countToSubtract;
    }

    private static StorageMovementEvent CreateStorageMovementEvent(
        StorageContent content,
        int count,
        StorageMovementType movementType)
    {
        return StorageMovementEvent.Create(new StorageMovementEventData
        {
            ProductId = content.ProductId,
            StorageName = content.StorageName,
            CurrencyId = content.CurrencyId,
            Count = count,
            BuyPrice = content.BuyPrice,
            MovementType = movementType
        });
    }
}
