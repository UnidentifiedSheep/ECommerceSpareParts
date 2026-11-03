using System.Data;
using Abstractions.Models.Options;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Interfaces.Services;
using Main.Entities.Sale;
using Main.Enums;
using Main.Enums.Balances;
using MediatR;
using Microsoft.Extensions.Options;

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
	ISender sender,
	IOptions<SystemOptions> systemOptions,
	IUnitOfWork unitOfWork,
	ISaleService saleService) : ICommandHandler<CreateSaleCommand, CreateSaleResult>
{
	public async Task<CreateSaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
	{
		var contents = request.Contents.ToList();

		var systemId = systemOptions.Value.SystemId;

		var totalSum = contents.Sum(x => x.DiscountedPrice * x.Count);

		if (request.PayedSum > 0)
			await sender.Send(
				new CreateTransactionCommand(
					request.OrganizationId,
					systemId,
					request.PayedSum.Value,
					request.CurrencyId,
					request.SaleDateTime,
					TransactionSourceType.Manual,
					TransactionCreationMode.System,
					request.ForcePayment),
				cancellationToken);

		var saleTransaction = (await sender.Send(
			new CreateTransactionCommand(
				systemId,
				request.OrganizationId,
				totalSum,
				request.CurrencyId,
				request.SaleDateTime,
				TransactionSourceType.Sale,
				TransactionCreationMode.System,
				request.ForcePayment),
			cancellationToken)).Transaction;

		var sale = Sale.Create(
			request.UserId,
			request.OrganizationId,
			saleTransaction.Id,
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

		sale.Complete();

		await unitOfWork.AddAsync(sale, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new CreateSaleResult(sale.Id);
	}
}
