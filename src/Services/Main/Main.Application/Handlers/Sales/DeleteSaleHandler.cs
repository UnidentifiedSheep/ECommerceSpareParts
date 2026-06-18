using System.Data;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Sale;
using Domain.Extensions;
using Exceptions;
using Main.Application.Handlers.Balance.ReverseTransaction;
using Main.Application.Handlers.StorageContents.RestoreContent;
using Main.Application.Interfaces.Persistence;
using Main.Application.Models;
using Main.Entities.Exceptions;
using Main.Entities.Sale;
using Main.Enums;
using Main.Enums.Balances;
using MediatR;

namespace Main.Application.Handlers.Sales;

[AutoSave]
[Transactional(IsolationLevel.Serializable, 20, 2)]
public record DeleteSaleCommand(Guid Id, uint RowVersion) : ICommand;

public class DeleteSaleHandler(
    ISaleRepository repository,
    IUnitOfWork unitOfWork,
    ISender sender,
    IIntegrationEventScope integrationEventScope
    ) : ICommandHandler<DeleteSaleCommand>
{
    public async Task<Unit> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        unitOfWork.Context.SuppressAutoSave = true; //we suppress auto save to make ont SaveChanges at the end.
                                                    //For details take a look at AutoSave attr.
        
        var sale = await repository.GetFullSaleForUpdate(request.Id, cancellationToken)
                   ?? throw new SaleNotFoundException(request.Id);

        if (sale.State == SaleState.Deleted)
            return Unit.Value;
        
        sale.ValidateVersion(request.RowVersion);
        sale.Delete();
        
        await sender.Send(
            new ReverseTransactionCommand(sale.TransactionId, TransactionReversalMode.System), 
            cancellationToken);
        
        await RestoreContents(sale, cancellationToken);
        
        integrationEventScope.Add(new SaleDeletedEvent
        {
            SaleId = sale.Id,
        });
        
        unitOfWork.Context.SuppressAutoSave = false; //return auth save, to save all changes.
        
        return Unit.Value;
    }

    private async Task RestoreContents(
        Sale sale,
        CancellationToken cancellationToken)
    {
        List<RestoreContentItem> toRestore = sale.Contents
            .SelectMany(content => content.Details
                .Select(detail => new RestoreContentItem(detail, content.ProductId)))
            .ToList();

        await sender.Send(
            new RestoreContentCommand(toRestore, StorageMovementType.SaleDeletion), 
            cancellationToken);
    }
}