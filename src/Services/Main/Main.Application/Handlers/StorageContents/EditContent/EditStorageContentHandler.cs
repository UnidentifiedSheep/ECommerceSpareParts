using System.Data;
using Abstractions.Interfaces.Persistence;
using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Currency;
using Attributes;
using Domain.Extensions;
using Main.Application.Dtos.Storage;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Enums;
using MediatR;
using Event = Main.Entities.Event.Event;

namespace Main.Application.Handlers.StorageContents.EditContent;

[AutoSave]
[Transactional(
    IsolationLevel.ReadCommitted,
    20,
    2)]
public record EditStorageContentCommand(
    Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>> EditedFields
) : ICommand;

public class EditStorageContentHandler(
    IStorageContentRepository storageContentRepository,
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

        foreach (var item in editedFields)
        {
            var patch = item.Value.Model;
            var content = storageContents[item.Key];

            content.ValidateVersion(item.Value.RowVersion);

            patch.Count.Apply(x => content.SetCount(x, StorageMovementType.StorageContentEditing));
            patch.CurrencyId.Apply(content.SetCurrencyId);

            if (patch.BuyPrice.IsSet)
            {
                var value = patch.BuyPrice.Value;
                var inBaseCurrency = await currencyConverter
                    .ConvertToBaseAsync(
                        value,
                        content.CurrencyId,
                        cancellationToken);
                content.SetBuyPrice(value, inBaseCurrency);
            }

            patch.PurchaseDatetime.Apply(content.SetPurchaseDate);
        }

        return Unit.Value;
    }
}