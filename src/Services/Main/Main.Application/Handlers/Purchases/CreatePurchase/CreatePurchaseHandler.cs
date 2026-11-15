using System.Data;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Main.Application.Dtos.Purchase;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Application.Interfaces.Services;
using Main.Entities.Purchase;
using Main.Entities.Storage;
using Main.Enums;
using Main.Enums.Balances;
using MediatR;

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
	ISender sender,
	IFinanceTransfers transfers,
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

	private async Task<IReadOnlyList<StorageContent>> AddContentsToStorage(
		CreatePurchaseCommand request,
		CancellationToken cancellationToken)
	{
		var command = new AddContentCommand(
			request.PurchaseContent
				.Select(x => new NewStorageContentDto
				{
					BuyPrice = x.Price,
					Count = x.Count,
					CurrencyId = request.CurrencyId,
					PurchaseDate = request.PurchaseDate,
					ProductId = x.ProductId
				})
				.ToArray(),
			request.StorageCode,
			StorageMovementType.Purchase);

		return (await sender.Send(command, cancellationToken)).StorageContents;
	}

	private async Task<Purchase> CreatePurchase(
		CreatePurchaseCommand request,
		Guid transactionId,
		IReadOnlyList<NewPurchaseContentDto> purchaseContents,
		IReadOnlyList<StorageContent> storageContents,
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
