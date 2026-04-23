using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Articles;
using Contracts.StorageContent;
using Main.Abstractions.Models;
using Main.Application.Extensions;
using Main.Application.Interfaces.Repositories;
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

        foreach (var (detail, articleId) in contentDetailsList)
        {
            productIds.Add(articleId);
            if (detail.StorageContentId == null) continue;
            storageContentIds.Add(detail.StorageContentId.Value);
        }
        var products = await productRepository
            .EnsureProductsExistsForUpdateAsync(productIds, cancellationToken);

        var storageContents = await contentRepository
            .EnsureStorageContentsExistsForUpdateAsync(storageContentIds, cancellationToken);

        var events = new List<Event>();

        foreach (var (detail, productId) in contentDetailsList)
        {
            Product product = products[productId];
            
            if (detail.StorageContentId != null)
            {
                var content = storageContents[detail.StorageContentId.Value];
                events.Add(StorageMovementEvent.Create(content, request.MovementType));
                content.IncreaseCount(detail.Count);
            }
            else
            {
                var content = StorageContent.Create(
                    detail.Storage,
                    productId,
                    detail.Count,
                    detail.BuyPrice,
                    detail.CurrencyId,
                    detail.PurchaseDatetime);
                
                events.Add(StorageMovementEvent.Create(content, request.MovementType));
                await unitOfWork.AddAsync(content, cancellationToken);
            }
            
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