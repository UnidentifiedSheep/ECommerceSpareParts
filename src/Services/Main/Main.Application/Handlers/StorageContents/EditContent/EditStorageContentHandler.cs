using System.Data;
using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Currency;
using Application.Common.Models;
using Attributes;
using Domain.Extensions;
using Main.Application.Dtos.Storage;
using Main.Application.Extensions;
using Main.Entities.Storage;
using Main.Enums;
using MediatR;

namespace Main.Application.Handlers.StorageContents.EditContent;

[AutoSave]
[Transactional(
	IsolationLevel.ReadCommitted,
	20,
	2)]
public record EditStorageContentCommand(
	Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>> EditedFields) : ICommand;

public class EditStorageContentHandler(
	IRepository<StorageContent, int> storageContentRepository,
	ICurrencyConverter currencyConverter,
	TimeProvider timeProvider) : ICommandHandler<EditStorageContentCommand>
{
	public async Task<Unit> Handle(EditStorageContentCommand request, CancellationToken cancellationToken)
	{
		var editedFields = request.EditedFields;

		var storageContents = await storageContentRepository.GetWithActiveLocksForUpdateAsync(
			editedFields.Keys, cancellationToken: cancellationToken);
		var utcNow = timeProvider.GetUtcNow().UtcDateTime;

		foreach (var item in editedFields)
		{
			var patch = item.Value.Model;
			var content = storageContents[item.Key];

			content.ValidateVersion(item.Value.RowVersion);

			patch.Count.Apply(x => content.SetCount(x, StorageMovementType.StorageContentEditing, utcNow));
			patch.CurrencyId.Apply(content.SetCurrencyId);

			if (patch.BuyPrice.IsSet)
			{
				var value = patch.BuyPrice.Value;
				var inBaseCurrency = await currencyConverter.ConvertToBaseAsync(
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
