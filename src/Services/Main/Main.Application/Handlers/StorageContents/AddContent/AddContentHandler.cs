using System.Data;
using Abstractions.Interfaces.Persistence;
using Abstractions.Interfaces.Services;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Settings;
using Attributes;
using Contracts.Products;
using Main.Application.Dtos.Storage;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services.Storage;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Entities.Settings;
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

public record AddContentResult(IReadOnlyList<StorageContent> StorageContents);

public class AddContentHandler(
    IProductRepository productRepository,
    ICurrencyConverter converter,
    ISettingsService settingsService,
    ICurrencyRepository currencyRepository,
    IUnitOfWork unitOfWork,
    IStorageContentChangeNotifier changeNotifier) : ICommandHandler<AddContentCommand, AddContentResult>
{
    public async Task<AddContentResult> Handle(AddContentCommand request, CancellationToken cancellationToken)
    {
        var baseCurrencyId = (await settingsService.GetOrDefault<CurrencySetting>(cancellationToken))
            .Data.BaseCurrencyId;

        var productIds = new HashSet<int>();
        var currencyIds = new HashSet<int>();

        foreach (var item in request.StorageContent)
        {
            productIds.Add(item.ProductId);
            currencyIds.Add(item.CurrencyId);
        }

        var products = await productRepository
            .EnsureExistsForUpdateAsync(
                productIds,
                notFound => new ProductNotFoundException(notFound),
                cancellationToken);

        var currencies = await currencyRepository
            .EnsureExistsAsync(
                currencyIds,
                nf => new CurrencyNotFoundException(nf),
                cancellationToken);

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
                await converter.ConvertToBaseAsync(item.BuyPrice, item.CurrencyId, cancellationToken),
                baseCurrencyId,
                item.PurchaseDate);

            content.AssignCurrency(currencies[item.CurrencyId]);

            storageContents.Add(content);

            var storageMovementEvent = StorageMovementEvent.Create(content, request.MovementType);
            events.Add(storageMovementEvent);

            products[item.ProductId].IncreaseStock(item.Count);
        }

        await unitOfWork.AddRangeAsync(storageContents, cancellationToken);
        await unitOfWork.AddRangeAsync(events, cancellationToken);

        changeNotifier.NotifyChanged(productIds);

        return new AddContentResult(storageContents);
    }
}