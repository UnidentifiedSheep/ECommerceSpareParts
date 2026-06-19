using System.Data;
using Abstractions.Interfaces.Persistence;
using Abstractions.Interfaces.Services;
using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Currency;
using Attributes;
using Contracts.Products;
using Contracts.StorageContent;
using Domain.Extensions;
using Main.Application.Dtos.Storage;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services.Storage;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Enums;
using MediatR;
using Event = Main.Entities.Event.Event;

namespace Main.Application.Handlers.StorageContents.EditContent;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record EditStorageContentCommand(
    Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>> EditedFields) : ICommand;

public class EditStorageContentHandler(
    IStorageContentRepository storageContentRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IStorageContentChangeNotifier changeNotifier,
    ICurrencyConverter currencyConverter
) : ICommandHandler<EditStorageContentCommand>
{
    public async Task<Unit> Handle(EditStorageContentCommand request, CancellationToken cancellationToken)
    {
        var editedFields = request.EditedFields;

        var storageContents = await storageContentRepository
            .EnsureExistsForUpdateAsync(
                editedFields.Keys,
                nf => new StorageContentNotFoundException(nf),
                cancellationToken);

        var products = await productRepository
            .EnsureExistsForUpdateAsync(
                storageContents.Select(x => x.Value.ProductId),
                nf => new ProductNotFoundException(nf),
                cancellationToken);

        var storageMovements = new List<Event>();

        foreach (var item in editedFields)
        {
            var patch = item.Value.Model;
            var content = storageContents[item.Key];
            var product = products[content.ProductId];

            content.ValidateVersion(item.Value.RowVersion);
            product.IncreaseStock(CalculateDiff(content, patch));

            var movementEvent = StorageMovementEvent.Create(content, StorageMovementType.StorageContentEditing);

            patch.Count.Apply(content.SetCount);
            patch.CurrencyId.Apply(content.SetCurrencyId);

            if (patch.BuyPrice.IsSet)
            {
                var value = patch.BuyPrice.Value;
                var inBaseCurrency = await currencyConverter
                    .ConvertToBaseAsync(value, content.CurrencyId, cancellationToken);
                content.SetBuyPrice(value, inBaseCurrency);
            }

            patch.PurchaseDatetime.Apply(content.SetPurchaseDate);

            storageMovements.Add(movementEvent);
        }

        await unitOfWork.AddRangeAsync(storageMovements, cancellationToken);

        changeNotifier.NotifyChanged(products.Keys);

        return Unit.Value;
    }

    private int CalculateDiff(
        StorageContent content,
        PatchStorageContentDto patch)
    {
        if (!patch.Count.IsSet || patch.Count.Value == content.Count) return 0;
        return patch.Count.Value - content.Count;
    }
}