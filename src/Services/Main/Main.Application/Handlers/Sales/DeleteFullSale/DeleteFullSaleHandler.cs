using System.Data;
using Application.Common.Interfaces;
using Contracts.Sale;
using Core.Attributes;
using Core.Interfaces;
using Core.Interfaces.Services;
using Main.Application.Handlers.Balance.DeleteTransaction;
using Main.Application.Handlers.Sales.DeleteSale;
using Main.Application.Handlers.StorageContents.RestoreContent;
using Main.Core.Dtos.Amw.Sales;
using Main.Core.Entities;
using Main.Core.Enums;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Sales.DeleteFullSale;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record DeleteFullSaleCommand(string SaleId, Guid UserId) : ICommand;

public class DeleteFullSaleHandler(IMediator mediator, IMessageBroker messageBroker, 
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteFullSaleCommand>
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

        await messageBroker.Publish(new SaleDeletedEvent(sale.Adapt<global::Contracts.Models.Sale.Sale>()), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task<Sale> DeleteAndGetSaleAsync(string saleId, CancellationToken cancellationToken = default)
    {
        var command = new DeleteSaleCommand(saleId);
        return (await mediator.Send(command, cancellationToken)).Sale;
    }

    private async Task DeleteTransaction(string transactionId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteTransactionCommand(transactionId, userId, true);
        await mediator.Send(command, cancellationToken);
    }

    private async Task RestoreStorageContents(IEnumerable<RestoreContentItem> details, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new RestoreContentCommand(details, StorageMovementType.SaleDeletion, userId);
        await mediator.Send(command, cancellationToken);
    }
}