using System.Data;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Enums;
using Main.Entities.Purchase.Exceptions;
using Main.Modules.Balance.Abstractions;
using Main.Modules.Storage.Abstractions.Interfaces;
using Main.Modules.Storage.Abstractions.Models;
using MediatR;

namespace Main.Modules.Purchase.Application.Handlers.Purchases.DeletePurchase;

[AutoSave]
[Transactional(
	IsolationLevel.Serializable,
	20,
	2)]
public record DeletePurchaseCommand(Guid PurchaseId) : ICommand<Unit>;

public class DeletePurchaseHandler(
	IUnitOfWork unitOfWork,
	IRepository<Entities.Purchase.Purchase.Purchase, Guid> repository,
	IFinanceTransfers transfers,
	IStorageContentService storageContentService) : ICommandHandler<DeletePurchaseCommand, Unit>
{
	public async Task<Unit> Handle(DeletePurchaseCommand request, CancellationToken cancellationToken)
	{
		var purchase = await GetPurchase(request.PurchaseId, cancellationToken);
		await SubtractStock(purchase, cancellationToken);
		await RevertTransactions(purchase, cancellationToken);

		unitOfWork.Remove(purchase);

		return Unit.Value;
	}

	private async Task<Entities.Purchase.Purchase.Purchase> GetPurchase(Guid id, CancellationToken cancellationToken)
		=> await repository.EnsureExistForUpdateAsync(
			id,
			key => new PurchaseNotFoundException(key),
			Criteria<Entities.Purchase.Purchase.Purchase>.New().Include(x => x.Contents),
			cancellationToken);

	private async Task SubtractStock(Entities.Purchase.Purchase.Purchase purchase, CancellationToken token)
	{
		var items =
			purchase.Contents
				.Select(x => new SubtractStorageContentItem(x.StorageContentId, x.Count))
				.ToArray();
		await storageContentService.SubtractByContentsAsync(
			items,
			StorageMovementType.PurchaseDeletion,
			token);
	}

	private Task RevertTransactions(Entities.Purchase.Purchase.Purchase purchase, CancellationToken token)
		=> transfers.ReverseAsync(purchase.TransactionId, token);
}
