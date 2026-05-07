using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Settings;
using Attributes;
using Contracts.StorageContent;
using Main.Abstractions.Models.Settings;
using Main.Application.Dtos.Storage;
using Main.Application.Extensions;
using Main.Application.Handlers.Projections;
using Main.Application.Interfaces.Persistence;
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
    ICurrencyConverter converter,
    ISettingsService settingsService,
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope) : ICommandHandler<AddContentCommand, AddContentResult>
{
    public async Task<AddContentResult> Handle(AddContentCommand request, CancellationToken cancellationToken)
    {
        var baseCurrencyId = (await settingsService.GetOrDefault<CurrencySetting>(cancellationToken))
            .Data.BaseCurrencyId;
        
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
                storageName: request.StorageName,
                productId: item.ProductId,
                count: item.Count,
                buyPrice: item.BuyPrice,
                currencyId: item.CurrencyId,
                buyPriceInBaseCurrency: await converter.ConvertToBaseAsync(item.BuyPrice, item.CurrencyId, cancellationToken),
                buyPriceInBaseCurrencyId: baseCurrencyId,
                purchaseDatetime: item.PurchaseDate);
            
            storageContents.Add(content);

            var storageMovementEvent = StorageMovementEvent.Create(content, request.MovementType);
            events.Add(storageMovementEvent);

            products[item.ProductId].IncreaseStock(item.Count);
        }

        await unitOfWork.AddRangeAsync(storageContents, cancellationToken);
        await unitOfWork.AddRangeAsync(events, cancellationToken);

        foreach (var id in productIds)
            integrationEventScope.Add(new StorageContentUpdatedEvent
            {
                ProductId = id
            });
        

        var adapted = storageContents
            .Select(StorageContentProjections.ToStorageContentDto.AsFunc())
            .ToList();
        
        return new AddContentResult(adapted);
    }
}