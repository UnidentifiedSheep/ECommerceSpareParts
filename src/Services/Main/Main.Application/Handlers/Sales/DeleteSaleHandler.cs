using System.Data;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Domain.Extensions;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Exceptions;
using Main.Enums;
using MediatR;

namespace Main.Application.Handlers.Sales;

[AutoSave]
[Transactional(
	IsolationLevel.Serializable,
	20,
	2)]
public record DeleteSaleCommand(Guid Id, uint RowVersion) : ICommand;

public class DeleteSaleHandler(
	ISaleRepository repository,
	IFinanceTransfers transfers,
	ISaleService saleService) : ICommandHandler<DeleteSaleCommand>
{
	public async Task<Unit> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
	{
		var sale = await repository.GetFullSaleForUpdate(request.Id, cancellationToken) ??
			throw new SaleNotFoundException(request.Id);

		if (sale.State == SaleState.Deleted)
			return Unit.Value;

		sale.ValidateVersion(request.RowVersion);
		sale.Delete();

		await transfers.ReverseAsync(sale.TransactionId, cancellationToken);

		await saleService.RestoreContents(
			sale,
			StorageMovementType.SaleDeletion,
			cancellationToken);

		return Unit.Value;
	}
}
