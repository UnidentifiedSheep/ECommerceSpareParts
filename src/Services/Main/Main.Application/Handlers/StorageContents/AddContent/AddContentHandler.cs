using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Articles;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Application.Extensions;
using Main.Application.Handlers.Currencies.Projections;
using Main.Application.Interfaces.Repositories;
using Main.Entities.Event;
using Main.Entities.Storage;
using Main.Enums;
using Event = Main.Entities.Event.Event;

namespace Main.Application.Handlers.StorageContents.AddContent;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record AddContentCommand(
    IEnumerable<NewStorageContentDto> StorageContent,
    string StorageName,
    StorageMovementType MovementType
    ) : ICommand<AddContentResult>;

public record AddContentResult(IReadOnlyList<StorageContentDto> StorageContents);

public class AddContentHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope) : ICommandHandler<AddContentCommand, AddContentResult>
{
    public async Task<AddContentResult> Handle(AddContentCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.StorageContent
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();
        var products = await productRepository
                .EnsureProductsExistsForUpdateAsync(productIds, cancellationToken);

        var storageContents = new List<StorageContent>();
        var events = new List<Event>();

        foreach (var item in request.StorageContent)
        {
            var content = StorageContent.Create(
                request.StorageName,
                item.ProductId,
                item.Count,
                item.BuyPrice,
                item.CurrencyId,
                item.PurchaseDate);
            
            storageContents.Add(content);

            var storageMovementEvent = StorageMovementEvent.Create(content, request.MovementType);
            events.Add(storageMovementEvent);

            products[item.ProductId].IncreaseStock(item.Count);
        }

        await unitOfWork.AddRangeAsync(storageContents, cancellationToken);
        await unitOfWork.AddRangeAsync(events, cancellationToken);

        foreach (var id in productIds)
            integrationEventScope.Add(new ProductBuyPricesUpdatedEvent
            {
                ProductId = id
            });
        

        var adapted = storageContents
            .Select(StorageContentProjections.ToStorageContentDtoFunc)
            .ToList();
        
        return new AddContentResult(adapted);
    }
}