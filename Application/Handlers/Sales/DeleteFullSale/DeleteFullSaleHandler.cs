using System.Data;
using Application.Handlers.Balance.DeleteTransaction;
using Application.Handlers.Sales.DeleteSale;
using Application.Handlers.StorageContents.RestoreContent;
using Application.Interfaces;
using Core.Attributes;
using Core.Dtos.Amw.Sales;
using Core.Entities;
using Core.Enums;
using Mapster;
using MediatR;

namespace Application.Handlers.Sales.DeleteFullSale;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record DeleteFullSaleCommand(string SaleId, string UserId) : ICommand;

public class DeleteFullSaleHandler(IMediator mediator) : ICommandHandler<DeleteFullSaleCommand>
{
    public async Task<Unit> Handle(DeleteFullSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await DeleteAndGetSaleAsync(request.SaleId, cancellationToken);
        var transactionId = sale.TransactionId;
        var saleContentDetails = sale.SaleContents
            .SelectMany(x => x.SaleContentDetails.Select(detail =>
                new RestoreContentItem(detail.Adapt<SaleContentDetailDto>(), x.ArticleId)))
            .ToList();

        await DeleteTransaction(transactionId, request.UserId, cancellationToken);
        await RestoreStorageContents(saleContentDetails, request.UserId, cancellationToken);
        return Unit.Value;
    }

    private async Task<Sale> DeleteAndGetSaleAsync(string saleId, CancellationToken cancellationToken = default)
    {
        var command = new DeleteSaleCommand(saleId);
        return (await mediator.Send(command, cancellationToken)).Sale;
    }

    private async Task DeleteTransaction(string transactionId, string userId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteTransactionCommand(transactionId, userId, true);
        await mediator.Send(command, cancellationToken);
    }

    private async Task RestoreStorageContents(IEnumerable<RestoreContentItem> details, string userId,
        CancellationToken cancellationToken = default)
    {
        var command = new RestoreContentCommand(details, StorageMovementType.SaleDeletion, userId);
        await mediator.Send(command, cancellationToken);
    }
}