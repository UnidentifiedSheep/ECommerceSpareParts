using System.Data;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Sale;
using Main.Application.Handlers.Balance.DeleteTransaction;
using Main.Application.Handlers.Sales.DeleteSale;
using Main.Application.Handlers.StorageContents.RestoreContent;
using Main.Application.Models;
using Main.Entities.Sale;
using Main.Enums;
using Mapster;
using MassTransit;
using MediatR;

namespace Main.Application.Handlers.Sales.DeleteFullSale;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record DeleteFullSaleCommand(Guid SaleId) : ICommand;

public class DeleteFullSaleHandler(
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork,
    IUserContext userContext) : ICommandHandler<DeleteFullSaleCommand>
{
    public async Task<Unit> Handle(DeleteFullSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await DeleteAndGetSaleAsync(request.SaleId, cancellationToken);
        var transactionId = sale.TransactionId;
        var saleContentDetails = sale.Contents
            .SelectMany(x => x.Details.Select(detail =>
                new RestoreContentItem(detail, x.ProductId)))
            .ToList();

        await ReverseTransaction(transactionId, userContext.UserId, cancellationToken);
        await RestoreStorageContents(saleContentDetails, cancellationToken);

        await publishEndpoint.Publish(new SaleDeletedEvent
        {
            Sale = sale.Adapt<Contracts.Models.Sale.Sale>()
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task<Sale> DeleteAndGetSaleAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        var command = new DeleteSaleCommand(saleId);
        return (await mediator.Send(command, cancellationToken)).Sale;
    }

    private async Task ReverseTransaction(
        Guid transactionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new ReverseTransactionCommand(transactionId, userId);
        await mediator.Send(command, cancellationToken);
    }

    private async Task RestoreStorageContents(
        IEnumerable<RestoreContentItem> details,
        CancellationToken cancellationToken = default)
    {
        var command = new RestoreContentCommand(details, StorageMovementType.SaleDeletion);
        await mediator.Send(command, cancellationToken);
    }
}