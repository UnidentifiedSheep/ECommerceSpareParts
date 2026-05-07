using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Articles;
using Contracts.StorageContent;
using Main.Application.Extensions;
using Main.Application.Interfaces.Persistence;
using Main.Application.Models;
using Main.Entities.Event;
using Main.Entities.Product;
using Main.Entities.Storage;
using Main.Enums;
using MediatR;

namespace Main.Application.Handlers.StorageContents.RestoreContent;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record RestoreContentCommand(
    IEnumerable<RestoreContentItem> ContentDetails,
    StorageMovementType MovementType) : ICommand;

public class RestoreContentHandler(
    IStorageContentRepository contentRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope) : ICommandHandler<RestoreContentCommand>
{
    public async Task<Unit> Handle(RestoreContentCommand request, CancellationToken cancellationToken)
    {
        var contentDetailsList = request.ContentDetails.ToList();
        var productIds = new HashSet<int>();
        var storageContentIds = new HashSet<int>();

        foreach (var (detail, productId) in contentDetailsList)
        {
            productIds.Add(productId);
            storageContentIds.Add(detail.StorageContentId);
        }
        var products = await productRepository
            .EnsureProductsExistsForUpdateAsync(productIds, cancellationToken);

        var storageContents = await contentRepository
            .EnsureStorageContentsExistsForUpdateAsync(storageContentIds, cancellationToken);

        var events = new List<Event>();

        foreach (var (detail, productId) in contentDetailsList)
        {
            Product product = products[productId];
            StorageContent content = storageContents[detail.StorageContentId];
            
            events.Add(StorageMovementEvent.Create(content, request.MovementType));
            
            content.IncreaseCount(detail.Count);
            product.IncreaseStock(detail.Count);
        }
        
        await unitOfWork.AddRangeAsync(events, cancellationToken);

        foreach (var id in productIds)
        {
            integrationEventScope.Add(new StorageContentUpdatedEvent
            {
                ProductId = id,
            });
            
            integrationEventScope.Add(new ProductUpdatedEvent
            {
                Id = id,
            });
        }

        return Unit.Value;
    }
}