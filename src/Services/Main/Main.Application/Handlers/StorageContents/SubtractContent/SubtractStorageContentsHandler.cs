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
using Main.Entities.Storage;
using Main.Enums;
using Event = Main.Entities.Event.Event;

namespace Main.Application.Handlers.StorageContents.SubtractContent;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record SubtractStorageContentsCommand(
    int StorageContentId,
    int Count,
    StorageMovementType MovementType) : ICommand<SubtractStorageContentsResult>;

public record SubtractStorageContentsResult(IReadOnlyList<SubtractedStorageContent> Contents);

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
        var firstContent = await storageContentRepository.EnsureExistForUpdateAsync(
            request.StorageContentId,
            id => new StorageContentNotFoundException(id),
            ct: cancellationToken);

        var product = await productRepository.EnsureExistForUpdateAsync(
            firstContent.ProductId,
            id => new ProductNotFoundException(id),
            ct: cancellationToken);

        var remaining = request.Count;
        var affected = new List<SubtractedStorageContent>();
        var events = new List<Event>();

        Subtract(firstContent, ref remaining, affected, events, request.MovementType);

        if (remaining > 0)
            await foreach (var content in storageContentRepository
                               .GetStorageContentsForUpdateAsync(
                                   firstContent.ProductId,
                                   firstContent.StorageName)
                               .WithCancellation(cancellationToken))
            {
                if (content.Id == firstContent.Id) continue;

                Subtract(content, ref remaining, affected, events, request.MovementType);
                if (remaining == 0) break;
            }

        if (remaining > 0)
        {
            var availableCount = request.Count - remaining;
            throw new NotEnoughCountOnStorageException(
                firstContent.ProductId,
                availableCount,
                request.Count);
        }

        product.IncreaseStock(-request.Count);
        await unitOfWork.AddRangeAsync(events, cancellationToken);

        integrationEventScope.Add(new ProductUpdatedEvent
        {
            Id = firstContent.ProductId
        });

        integrationEventScope.Add(new StorageContentUpdatedEvent
        {
            ProductId = firstContent.ProductId
        });

        return new SubtractStorageContentsResult(affected);
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