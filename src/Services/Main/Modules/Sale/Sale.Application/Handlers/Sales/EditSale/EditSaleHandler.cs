using Enums;
using System.Data;
using Abstractions.Models.Options;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Domain.Extensions;
using Main.Application.Dtos.Sale;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Modules.Balance.Abstractions;
using Main.Entities.Exceptions;
using Main.Entities.Sale;
using Main.Enums;
using Main.Enums.Balances;
using MediatR;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Sales.EditSale;

[AutoSave]
[Transactional(
	IsolationLevel.ReadCommitted,
	20,
	2)]
public record EditSaleCommand(
	Guid SaleId,
	uint RowVersion,
	IReadOnlyCollection<EditSaleContentDto> Content,
	int CurrencyId,
	DateTime SaleDateTime,
	string? Comment,
	bool ForcePayment = false) : ICommand;

public class EditSaleHandler(
	IFinanceTransfers transfers,
	ISaleRepository saleRepository,
	ISaleService saleService,
	IUnitOfWork unitOfWork) : ICommandHandler<EditSaleCommand>
{
	public async Task<Unit> Handle(EditSaleCommand request, CancellationToken cancellationToken)
	{
		var sale = await saleRepository.GetFullSaleForUpdate(request.SaleId, cancellationToken) ??
			throw new SaleNotFoundException(request.SaleId);

		if (sale.State == SaleState.Deleted)
			throw new SaleNotFoundException(request.SaleId);

		sale.ValidateVersion(request.RowVersion);

		var contentDtos = request.Content.ToList();

		await saleService.RestoreContents(
			sale,
			StorageMovementType.SaleEditing,
			cancellationToken);

		await ReverseSaleTransaction(sale, cancellationToken);

		var totalSum = contentDtos.Sum(x => x.PriceWithDiscount * x.Count);
		var transactionId = await CreateSaleTransaction(
			sale.OrganizationId,
			totalSum,
			request.CurrencyId,
			request.SaleDateTime,
			request.ForcePayment,
			cancellationToken);

		sale.SetTransactionId(transactionId);
		sale.SetCurrency(request.CurrencyId);
		sale.SetDateTime(request.SaleDateTime);
		sale.SetComment(request.Comment);

		await UpdateContents(
			sale,
			contentDtos,
			false,
			cancellationToken);

		if (sale.CalculateTotal() != totalSum)
			throw new InvalidOperationException("Sale total does not match the charged amount");

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return Unit.Value;
	}

	private Task ReverseSaleTransaction(Sale sale, CancellationToken cancellationToken)
		=> transfers.ReverseAsync(sale.TransactionId, cancellationToken);

	private Task<Guid> CreateSaleTransaction(
		Guid organizationId,
		decimal amount,
		int currencyId,
		DateTime transactionDateTime,
		bool forcePayment,
		CancellationToken cancellationToken)
		=> transfers.TransferAsync(
			transfers.SystemOrganizationId,
			organizationId,
			amount,
			currencyId,
			transactionDateTime,
			TransactionSourceType.Sale,
			forcePayment,
			cancellationToken);

	private async Task UpdateContents(
		Sale sale,
		IReadOnlyList<EditSaleContentDto> contentDtos,
		bool sellFromOtherStorages,
		CancellationToken cancellationToken)
	{
		var existingById = sale.Contents.ToDictionary(x => x.Id);
		var requestedIds = contentDtos.Where(x => x.Id is not null).Select(x => x.Id!.Value).ToHashSet();

		requestedIds.EnsureAllExists(existingById.Keys, ids => new SaleContentNotFoundException(ids[0]));

		var distributed = await saleService.TakeFromStorageAndDistributeDetails(
			sale.StorageCode,
			contentDtos,
			StorageMovementType.SaleEditing,
			cancellationToken);

		foreach (var removed in sale.Contents.Where(x => !requestedIds.Contains(x.Id)).ToList())
			RemoveContent(sale, removed);

		for (var i = 0; i < contentDtos.Count; i++)
		{
			var dto = contentDtos[i];
			var newContent = distributed[i];

			if (dto.Id is null)
			{
				sale.AddContent(newContent);
				continue;
			}

			UpdateContent(
				existingById[dto.Id.Value],
				dto,
				newContent);
		}
	}

	private void UpdateContent(
		SaleContent content,
		EditSaleContentDto dto,
		SaleContent newContent)
	{
		if (content.ProductId != dto.ProductId)
			throw new SaleContentProductMismatchException(content.Id);

		unitOfWork.RemoveRange(content.Details.ToList());
		content.SetPriceAndDetails(
			dto.Price,
			dto.PriceWithDiscount,
			newContent.Details);
		content.SetComment(dto.Comment);
	}

	private void RemoveContent(Sale sale, SaleContent content)
	{
		sale.RemoveContent(content);
		unitOfWork.Remove(content);
	}
}
