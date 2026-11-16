using Enums;
using System.Data;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Main.Application.Dtos.Sale;
using Main.Application.Interfaces.Services;
using Main.Modules.Balance.Abstractions;
using Main.Entities.Sale;
using Main.Enums;
using Main.Enums.Balances;

namespace Main.Application.Handlers.Sales.CreateSale;

[Transactional(
	IsolationLevel.ReadCommitted,
	30,
	2)]
[AutoSave]
public record CreateSaleCommand(
	Guid UserId,
	Guid OrganizationId,
	int CurrencyId,
	string StorageCode,
	DateTime SaleDateTime,
	IReadOnlyCollection<NewSaleContentDto> Contents,
	string? Comment,
	decimal? PayedSum,
	bool ForcePayment = false) : ICommand<CreateSaleResult>;

public record CreateSaleResult(Guid SaleId);

public class CreateSaleHandler(
	IFinanceTransfers transfers,
	IUnitOfWork unitOfWork,
	ISaleService saleService) : ICommandHandler<CreateSaleCommand, CreateSaleResult>
{
	public async Task<CreateSaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
	{
		var contents = request.Contents.ToList();

		var systemId = transfers.SystemOrganizationId;

		var totalSum = contents.Sum(x => x.DiscountedPrice * x.Count);

		if (request.PayedSum > 0)
			await transfers.TransferAsync(
				request.OrganizationId,
				systemId,
				request.PayedSum.Value,
				request.CurrencyId,
				request.SaleDateTime,
				TransactionSourceType.Manual,
				request.ForcePayment,
				cancellationToken);

		var saleTransactionId = await transfers.TransferAsync(
			systemId,
			request.OrganizationId,
			totalSum,
			request.CurrencyId,
			request.SaleDateTime,
			TransactionSourceType.Sale,
			request.ForcePayment,
			cancellationToken);

		var sale = Sale.Create(
			request.UserId,
			request.OrganizationId,
			saleTransactionId,
			request.CurrencyId,
			request.StorageCode,
			request.SaleDateTime);

		sale.SetComment(request.Comment);

		var distributed = await saleService.TakeFromStorageAndDistributeDetails(
			request.StorageCode,
			contents,
			StorageMovementType.Sale,
			cancellationToken);

		foreach (var content in distributed)
			sale.AddContent(content);

		if (sale.CalculateTotal() != totalSum)
			throw new InvalidOperationException("Sale total does not match the charged amount");

		sale.Complete();

		await unitOfWork.AddAsync(sale, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new CreateSaleResult(sale.Id);
	}
}
