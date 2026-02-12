using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Sale;
using Main.Abstractions.Dtos.Amw.Sales;
using Main.Abstractions.Models;
using Main.Application.Handlers.Balance.DeleteTransaction;
using Main.Application.Handlers.Sales.DeleteSale;
using Main.Application.Handlers.StorageContents.RestoreContent;
using Main.Entities;
using Main.Enums;
using Mapster;
using MassTransit;
using MediatR;

namespace Main.Application.Handlers.Sales.DeleteFullSale;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record DeleteFullSaleCommand(string SaleId, Guid UserId) : ICommand;

public class DeleteFullSaleHandler(IMediator mediator, IPublishEndpoint publishEndpoint, 
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

        await publishEndpoint.Publish(new SaleDeletedEvent
        {
            Sale = sale.Adapt<global::Contracts.Models.Sale.Sale>()
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task<Sale> DeleteAndGetSaleAsync(string saleId, CancellationToken cancellationToken = default)
    {
        var command = new DeleteSaleCommand(saleId);
        return (await mediator.Send(command, cancellationToken)).Sale;
    }

    private async Task DeleteTransaction(Guid transactionId, Guid userId,
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