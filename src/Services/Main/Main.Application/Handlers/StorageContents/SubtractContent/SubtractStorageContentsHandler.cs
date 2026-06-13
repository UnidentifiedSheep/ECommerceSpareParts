using System.Data;
using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Settings;
using Attributes;
using Contracts.Products;
using Contracts.StorageContent;
using Main.Application.Interfaces.Persistence;
using Main.Application.NamedObjects.StorageContentExtractPolicies;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Main.Entities.Setting;
using Main.Entities.Storage;
using Main.Enums;
using Event = Main.Entities.Event.Event;

namespace Main.Application.Handlers.StorageContents.SubtractContent;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record SubtractStorageContentsCommand(
    IEnumerable<ISubtractStorageContentItem> Items,
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

public record SubtractedStorageContent(int StorageContentId, int Count);

public class SubtractStorageContentsHandler(
    IStorageContentRepository storageContentRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope,
    ISettingsService settingsService,
    INamedObjectRegistry<StorageContentExtractPolicyBase> policyRegistry)
    : ICommandHandler<SubtractStorageContentsCommand, SubtractStorageContentsResult>
{
    public async Task<SubtractStorageContentsResult> Handle(
        SubtractStorageContentsCommand request,
        CancellationToken cancellationToken)
    {
        var items = request.Items.ToList();

        //StorageContentId | Count
        var byStorageContents = new Dictionary<int, int>();

        //Key = StorageName + productId, Value = Count
        var byProductAndStorage = new Dictionary<(string storageName, int productId), int>();

        foreach (var item in items)
        {
            switch (item)
            {
                case SubtractProductFromStorageItem byProduct:
                    var key = (byProduct.StorageName, byProduct.ProductId);
                    byProductAndStorage[key] = byProductAndStorage.GetValueOrDefault(key) + byProduct.Count;
                    break;
                case SubtractStorageContentItem byContent:
                    byStorageContents[byContent.StorageContentId] =
                        byStorageContents.GetValueOrDefault(byContent.StorageContentId) + byContent.Count;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported subtract item type {item.GetType().Name}.");
            }
        }

        var storageContents = byStorageContents.Count == 0
            ? new Dictionary<int, StorageContent>()
            : await storageContentRepository
                .EnsureExistsForUpdateAsync(
                    byStorageContents.Keys,
                    ids => new StorageContentNotFoundException(ids[0]),
                    cancellationToken);

        var allProductIds = storageContents
            .Values
            .Select(x => x.ProductId)
            .Concat(byProductAndStorage.Keys.Select(x => x.productId))
            .Distinct()
            .ToList();

        var products = await productRepository.EnsureExistsForUpdateAsync(
            allProductIds,
            ids => new ProductNotFoundException(ids),
            cancellationToken);

        var events = new List<Event>();
        var affected = new List<SubtractedStorageContent>();
        var policy = await GetExtractionPolicy(cancellationToken);

        await SubtractByStorageContentsAsync(
            byStorageContents,
            storageContents,
            products,
            affected,
            events,
            request.MovementType,
            policy,
            cancellationToken);

        await SubtractByProductAndStorageAsync(
            byProductAndStorage,
            products,
            affected,
            events,
            request.MovementType,
            policy,
            cancellationToken);

        await unitOfWork.AddRangeAsync(events, cancellationToken);

        foreach (var (productId, _) in products)
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

    private async Task SubtractByStorageContentsAsync(
        Dictionary<int, int> byStorageContents,
        Dictionary<int, StorageContent> storageContents,
        Dictionary<int, Product> products,
        List<SubtractedStorageContent> affected,
        List<Event> events,
        StorageMovementType movementType,
        StorageContentExtractPolicyBase policy,
        CancellationToken cancellationToken)
    {
        foreach (var (contentId, count) in byStorageContents)
        {
            var remaining = count;
            var content = storageContents[contentId];

            Subtract(content, ref remaining, affected, events, movementType);
            await SubtractFromStorageContentsAsync(
                remaining,
                count,
                content.ProductId,
                content.StorageName,
                policy,
                events,
                affected,
                movementType,
                contentId,
                cancellationToken);

            products[content.ProductId].IncreaseStock(-count);
        }
    }

    private async Task SubtractByProductAndStorageAsync(
        Dictionary<(string storageName, int productId), int> byProductAndStorage,
        Dictionary<int, Product> products,
        List<SubtractedStorageContent> affected,
        List<Event> events,
        StorageMovementType movementType,
        StorageContentExtractPolicyBase policy,
        CancellationToken cancellationToken)
    {
        foreach (var ((storage, productId), count) in byProductAndStorage)
        {
            await SubtractFromStorageContentsAsync(
                count,
                count,
                productId,
                storage,
                policy,
                events,
                affected,
                movementType,
                null,
                cancellationToken);

            products[productId].IncreaseStock(-count);
        }
    }

    private async Task SubtractFromStorageContentsAsync(
        int count,
        int requestedCount,
        int productId,
        string storageName,
        StorageContentExtractPolicyBase policy,
        List<Event> events,
        List<SubtractedStorageContent> affected,
        StorageMovementType movementType,
        int? skipStorageContentId = null,
        CancellationToken cancellationToken = default)
    {
        int remaining = count;

        if (remaining > 0)
            await foreach (var content in storageContentRepository
                               .GetStorageContentsForUpdateAsync(
                                   productId,
                                   storageName,
                                   policy: policy)
                               .WithCancellation(cancellationToken))
            {
                if (skipStorageContentId.HasValue && content.Id == skipStorageContentId)
                    continue;

                Subtract(content, ref remaining, affected, events, movementType);
                if (remaining == 0) break;
            }

        if (remaining > 0)
        {
            var availableCount = requestedCount - remaining;
            throw new NotEnoughCountOnStorageException(
                productId,
                availableCount,
                requestedCount);
        }
    }

    private async Task<StorageContentExtractPolicyBase> GetExtractionPolicy(CancellationToken cancellationToken)
    {
        var setting = (await settingsService.GetOrDefault<StorageContentSetting>(cancellationToken)).Data;
        return policyRegistry.GetBySystemName(setting.StorageContentExtractionPolicy);
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
