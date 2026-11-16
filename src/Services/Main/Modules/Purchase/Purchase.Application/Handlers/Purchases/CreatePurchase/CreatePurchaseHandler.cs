using Enums;
using System.Data;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Main.Application.Dtos.Purchase;
using Main.Application.Interfaces.Services;
using Main.Modules.Balance.Abstractions;
using Main.Application.Models.Storage;
using Main.Entities.Purchase;
using Main.Enums;
using Main.Enums.Balances;

namespace Main.Application.Handlers.Purchases.CreatePurchase;

[AutoSave]
[Transactional(
	IsolationLevel.ReadCommitted,
	20,
	2)]
public record CreatePurchaseCommand(
	Guid SupplierUserId,
	Guid SupplierOrganizationId,
	int CurrencyId,
	string StorageCode,
	DateTime PurchaseDate,
	IReadOnlyCollection<NewPurchaseContentDto> PurchaseContent,
	string? Comment,
	decimal? PayedSum,
	bool ForcePayment = false) : ICommand<CreatePurchaseResult>;

public record CreatePurchaseResult(Guid PurchaseId);

public class CreatePurchaseHandler(
	IFinanceTransfers transfers,
	IStorageContentService storageContentService,
	IUnitOfWork unitOfWork) : ICommandHandler<CreatePurchaseCommand, CreatePurchaseResult>
{
	public async Task<CreatePurchaseResult> Handle(
		CreatePurchaseCommand request,
		CancellationToken cancellationToken)
	{
		var systemId = transfers.SystemOrganizationId;

		var purchaseContents = request.PurchaseContent.ToList();

		var totalSum = purchaseContents.Sum(x => x.Price * x.Count);

		var purchaseTransactionId = await transfers.TransferAsync(
			request.SupplierOrganizationId,
			systemId,
			totalSum,
			request.CurrencyId,
			request.PurchaseDate,
			TransactionSourceType.Purchase,
			cancellationToken: cancellationToken);

		var storageContents = await AddContentsToStorage(request, cancellationToken);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		var purchase = await CreatePurchase(
			request,
			purchaseTransactionId,
			purchaseContents,
			storageContents,
			cancellationToken);

		if (request.PayedSum > 0)
			await transfers.TransferAsync(
				systemId,
				request.SupplierOrganizationId,
				request.PayedSum.Value,
				request.CurrencyId,
				request.PurchaseDate,
				TransactionSourceType.Manual,
				request.ForcePayment,
				cancellationToken);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new CreatePurchaseResult(purchase.Id);
	}

	private async Task<IReadOnlyList<StoredLot>> AddContentsToStorage(
		CreatePurchaseCommand request,
		CancellationToken cancellationToken)
	{
		return await storageContentService.AddAsync(
			request.PurchaseContent
				.Select(x => new NewStorageLot(
					x.ProductId,
					request.CurrencyId,
					x.Price,
					x.Count,
					request.PurchaseDate))
				.ToArray(),
			request.StorageCode,
			StorageMovementType.Purchase,
			cancellationToken);
	}

	private async Task<Purchase> CreatePurchase(
		CreatePurchaseCommand request,
		Guid transactionId,
		IReadOnlyList<NewPurchaseContentDto> purchaseContents,
		IReadOnlyList<StoredLot> storageContents,
		CancellationToken cancellationToken)
	{
		var purchase = Purchase.Create(
			request.SupplierUserId,
			request.SupplierOrganizationId,
			request.CurrencyId,
			transactionId,
			request.StorageCode,
			request.PurchaseDate);

		purchase.SetComment(request.Comment);

		for (var i = 0; i < storageContents.Count; i++)
		{
			var content = purchaseContents[i];
			var storageContent = storageContents[i];

			if (content.ProductId != storageContent.ProductId)
				throw new InvalidOperationException(
					"Order of storage contents and " + "new purchase contents are invalid.");

			var purchaseContent = PurchaseContent.Create(
				content.ProductId,
				content.Count,
				content.Price,
				storageContent.Id,
				content.Comment);

			purchase.AddContent(purchaseContent);

		}

		purchase.Complete();
		await unitOfWork.AddAsync(purchase, cancellationToken);
		return purchase;
	}
}
