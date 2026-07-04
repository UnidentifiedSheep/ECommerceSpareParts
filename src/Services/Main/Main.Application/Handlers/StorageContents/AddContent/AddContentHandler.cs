using System.Data;
using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Settings;
using Attributes;
using Main.Application.Dtos.Storage;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Entities.Settings;
using Main.Entities.Storage;
using Main.Enums;
using Event = Main.Entities.Event.Event;

namespace Main.Application.Handlers.StorageContents.AddContent;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted,
    20,
    2)]
public record AddContentCommand(
    IEnumerable<NewStorageContentDto> StorageContent,
    string StorageName,
    StorageMovementType MovementType
) : ICommand<AddContentResult>;

public record AddContentResult(IReadOnlyList<StorageContent> StorageContents);

public class AddContentHandler(
    ICurrencyConverter converter,
    ISettingsService settingsService,
    ICurrencyRepository currencyRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<AddContentCommand, AddContentResult>
{
    public async Task<AddContentResult> Handle(AddContentCommand request, CancellationToken cancellationToken)
    {
        var baseCurrencyId = (await settingsService.GetOrDefault<CurrencySetting>(cancellationToken))
            .Data.BaseCurrencyId;

        var currencyIds = new HashSet<int>();

        foreach (var item in request.StorageContent)
        {
            currencyIds.Add(item.CurrencyId);
        }

        var currencies = await currencyRepository
            .EnsureExistsAsync(
                currencyIds,
                nf => new CurrencyNotFoundException(nf),
                cancellationToken);
        
        await productRepository.EnsureExistsAsync(
            request.StorageContent.Select(x => x.ProductId).Distinct(),
            nf => new ProductNotFoundException(nf),
            cancellationToken);

        var storageContents = new List<StorageContent>();

        foreach (var item in request.StorageContent)
        {
            var content = StorageContent.Create(
                request.StorageName,
                item.ProductId,
                item.BuyPrice,
                item.CurrencyId,
                await converter.ConvertToBaseAsync(
                    item.BuyPrice,
                    item.CurrencyId,
                    cancellationToken),
                baseCurrencyId,
                item.PurchaseDate);
            
            content.SetCount(item.Count, request.MovementType);
            content.AssignCurrency(currencies[item.CurrencyId]);
            storageContents.Add(content);
        }

        await unitOfWork.AddRangeAsync(storageContents, cancellationToken);

        return new AddContentResult(storageContents);
    }
}
