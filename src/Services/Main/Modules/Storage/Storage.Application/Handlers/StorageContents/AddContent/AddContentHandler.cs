using System.Data;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Storage;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Modules.Currency.Abstractions;
using Main.Enums;

namespace Main.Application.Handlers.StorageContents.AddContent;

[AutoSave]
[Transactional(
	IsolationLevel.ReadCommitted,
	20,
	2)]
public record AddContentCommand(
	IReadOnlyCollection<NewStorageContentDto> StorageContent,
	string StorageCode,
	StorageMovementType MovementType) : ICommand<AddContentResult>;

public record AddContentResult(IReadOnlyList<StorageContent> StorageContents);

public class AddContentHandler(
	ICurrencyConverter converter,
	ICurrencyLookup currencyLookup,
	IUnitOfWork unitOfWork) : ICommandHandler<AddContentCommand, AddContentResult>
{
	public async Task<AddContentResult> Handle(AddContentCommand request, CancellationToken cancellationToken)
	{
		var baseCurrencyId = await currencyLookup.GetBaseCurrencyIdAsync(cancellationToken);

		var storageContents = new List<StorageContent>();

		foreach (var item in request.StorageContent)
		{
			var content = StorageContent.Create(
				request.StorageCode,
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
			storageContents.Add(content);
		}

		await unitOfWork.AddRangeAsync(storageContents, cancellationToken);

		return new AddContentResult(storageContents);
	}
}
