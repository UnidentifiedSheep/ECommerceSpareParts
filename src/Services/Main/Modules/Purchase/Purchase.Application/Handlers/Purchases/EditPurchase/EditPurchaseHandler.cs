using System.Data;
using Abstractions.Models.Options;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Purchase;
using Main.Application.Interfaces.Services;
using Main.Modules.Balance.Abstractions;
using Main.Application.Models.Storage;
using Main.Entities.Exceptions;
using Main.Entities.Purchase;
using Main.Enums;
using Main.Enums.Balances;
using MediatR;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Purchases.EditPurchase;

[AutoSave]
[Transactional(
	IsolationLevel.ReadCommitted,
	20,
	2)]
public record EditPurchaseCommand(
	IReadOnlyCollection<EditPurchaseDto> Content,
	Guid PurchaseId,
	int CurrencyId,
	string? Comment,
	DateTime PurchaseDateTime) : ICommand;

public class EditPurchaseHandler(
	IFinanceTransfers transfers,
	IRepository<Purchase, Guid> purchaseRepository,
	IStorageContentService storageContentService,
	IUnitOfWork unitOfWork) : ICommandHandler<EditPurchaseCommand>
{
	public async Task<Unit> Handle(EditPurchaseCommand request, CancellationToken cancellationToken)
	{
		var purchaseId = request.PurchaseId;
		var purchase = await GetPurchase(purchaseId, cancellationToken);
		var contentDtos = request.Content.ToList();

		await ReversePurchaseTransactions(purchase, cancellationToken);

		var totalSum = contentDtos.Sum(x => x.Price * x.Count);
		var purchaseTransactionId = await CreateTransaction(
			purchase.SupplierOrganizationId,
			GetSystemUserId(),
			totalSum,
			request.CurrencyId,
			request.PurchaseDateTime,
			cancellationToken);

		purchase.SetTransactionId(purchaseTransactionId);
		purchase.SetCurrencyId(request.CurrencyId);
		purchase.SetPurchaseDate(request.PurchaseDateTime);
		purchase.SetComment(request.Comment);

		await UpdateContents(
			purchase,
			contentDtos,
			request,
			cancellationToken);

		return Unit.Value;
	}

	private Guid GetSystemUserId() => transfers.SystemOrganizationId;

	private Task<Purchase> GetPurchase(Guid purchaseId, CancellationToken cancellationToken)
		=> purchaseRepository.EnsureExistForUpdateAsync(
			purchaseId,
			id => new PurchaseNotFoundException(id),
			Criteria<Purchase>.New().Include(x => x.Contents),
			cancellationToken);

	private Task ReversePurchaseTransactions(Purchase purchase, CancellationToken cancellationToken)
		=> transfers.ReverseAsync(purchase.TransactionId, cancellationToken);

	private Task<Guid> CreateTransaction(
		Guid senderId,
		Guid receiverId,
		decimal amount,
		int currencyId,
		DateTime transactionDateTime,
		CancellationToken cancellationToken)
		=> transfers.TransferAsync(
			senderId,
			receiverId,
			amount,
			currencyId,
			transactionDateTime,
			TransactionSourceType.Purchase,
			cancellationToken: cancellationToken);

	private async Task UpdateContents(
		Purchase purchase,
		IReadOnlyList<EditPurchaseDto> contentDtos,
		EditPurchaseCommand request,
		CancellationToken cancellationToken)
	{
		var existingById = purchase.Contents.ToDictionary(x => x.Id);
		var requestedIds = contentDtos.Where(x => x.Id is not null).Select(x => x.Id!.Value).ToHashSet();

		requestedIds.EnsureAllExists(existingById.Keys, ids => new PurchaseContentNotFoundException(ids[0]));

		var storageContentIds = purchase.Contents.Select(x => x.StorageContentId).ToHashSet();

		var storageContents = await storageContentService.LockAsync(storageContentIds, cancellationToken);

		var deletionSubtractions = new List<SubtractStorageContentItem>();
		var editingSubtractions = new List<SubtractStorageContentItem>();
		var editingRestorations = new List<RestoreStorageLot>();
		var contentToAdd = new List<EditPurchaseDto>();

		foreach (var removed in purchase.Contents.Where(x => !requestedIds.Contains(x.Id)).ToList())
			RemoveContent(
				purchase,
				removed,
				deletionSubtractions);

		foreach (var dto in contentDtos)
		{
			if (dto.Id is null)
			{
				contentToAdd.Add(dto);
				continue;
			}

			await UpdateContent(
				existingById[dto.Id.Value],
				dto,
				request,
				storageContents,
				editingRestorations,
				editingSubtractions,
				cancellationToken);
		}

		if (contentToAdd.Count > 0)
			await AddContent(
				purchase,
				contentToAdd,
				cancellationToken);

		if (deletionSubtractions.Count > 0)
			await storageContentService.SubtractByContentsAsync(
				deletionSubtractions,
				StorageMovementType.PurchaseEditing,
				cancellationToken);

		if (editingSubtractions.Count > 0)
			await storageContentService.SubtractByContentsAsync(
				editingSubtractions,
				StorageMovementType.PurchaseEditing,
				cancellationToken);

		if (editingRestorations.Count > 0)
			await storageContentService.RestoreAsync(
				editingRestorations,
				StorageMovementType.PurchaseEditing,
				cancellationToken);
	}

	private void RemoveContent(
		Purchase purchase,
		PurchaseContent content,
		ICollection<SubtractStorageContentItem> subtractions)
	{
		subtractions.Add(new SubtractStorageContentItem(content.StorageContentId, content.Count));

		purchase.RemoveContent(content);
		unitOfWork.Remove(content);
	}

	private async Task AddContent(
		Purchase purchase,
		List<EditPurchaseDto> newContents,
		CancellationToken cancellationToken)
	{
		var lots = await storageContentService.AddAsync(
			newContents
				.Select(x => new NewStorageLot(
					x.ProductId,
					purchase.CurrencyId,
					x.Price,
					x.Count,
					purchase.PurchaseDatetime))
				.ToArray(),
			purchase.Storage,
			StorageMovementType.PurchaseEditing,
			cancellationToken);

		if (lots.Count != newContents.Count)
			throw new InvalidOperationException(
				"Storage returned unexpected number of storage contents.");

		for (var i = 0; i < lots.Count; i++)
		{
			var storageContent = lots[i];
			var newContent = newContents[i];

			if (storageContent.ProductId != newContent.ProductId || storageContent.Count != newContent.Count)
				throw new InvalidOperationException("Unexpected storage content when updating purchase.");

			purchase.AddContent(
				PurchaseContent.Create(
					storageContent.ProductId,
					storageContent.Count,
					storageContent.BuyPrice,
					storageContent.Id,
					newContent.Comment));
		}
	}

	private async Task UpdateContent(
		PurchaseContent content,
		EditPurchaseDto dto,
		EditPurchaseCommand request,
		IReadOnlyDictionary<int, StoredLot> storageContents,
		List<RestoreStorageLot> restorations,
		List<SubtractStorageContentItem> subtractions,
		CancellationToken cancellationToken)
	{
		if (content.ProductId != dto.ProductId)
			throw new ArticleDoesntMatchContentException(content.Id);

		var storageContent = storageContents[content.StorageContentId];

		var lotIsUntouched = storageContent.Count == content.Count;
		var countDelta = dto.Count - content.Count;
		if (countDelta < 0)
			subtractions.Add(new SubtractStorageContentItem(content.StorageContentId, -countDelta));
		else if (countDelta > 0)
			restorations.Add(
				new RestoreStorageLot(
					content.StorageContentId,
					content.ProductId,
					request.CurrencyId,
					dto.Price,
					countDelta));

		content.SetCount(dto.Count);
		content.SetPrice(dto.Price);
		content.SetComment(dto.Comment);

		if (!lotIsUntouched)
			return;

		await storageContentService.UpdatePurchaseInfoAsync(
			content.StorageContentId,
			request.CurrencyId,
			request.PurchaseDateTime,
			dto.Price,
			cancellationToken);
	}
}
