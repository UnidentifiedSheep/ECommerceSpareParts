using System.Data;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Attributes;
using Contracts.Sale;
using Domain.Extensions;
using Main.Application.Handlers.Balance.ReverseTransaction;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Exceptions;
using Main.Enums;
using Main.Enums.Balances;
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
    ISender sender,
    IIntegrationEventScope integrationEventScope,
    ISaleService saleService
) : ICommandHandler<DeleteSaleCommand>
{
    public async Task<Unit> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await repository.GetFullSaleForUpdate(request.Id, cancellationToken)
                   ?? throw new SaleNotFoundException(request.Id);

        if (sale.State == SaleState.Deleted) return Unit.Value;

        sale.ValidateVersion(request.RowVersion);
        sale.Delete();

        await sender.Send(
            new ReverseTransactionCommand(
                sale.TransactionId,
                TransactionReversalMode.System,
                true),
            cancellationToken);

        await saleService.RestoreContents(
            sale,
            StorageMovementType.SaleDeletion,
            cancellationToken);

        integrationEventScope.Add(
            new SaleDeletedEvent
            {
                SaleId = sale.Id
            });

        return Unit.Value;
    }
}