using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Purchase;
using Main.Application.Handlers.Balance.DeleteTransaction;
using Main.Application.Handlers.Purchases.DeletePurchase;
using Main.Application.Handlers.StorageContents.RemoveContent;
using Main.Entities;
using Main.Entities.Exceptions.Purchase;
using Main.Entities.Purchase;
using Main.Enums;
using Mapster;
using MassTransit;
using MediatR;
using ContractPurchase = Contracts.Models.Purchase.Purchase;

namespace Main.Application.Handlers.Purchases.DeleteFullPurchase;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record DeleteFullPurchaseCommand(string PurchaseId, Guid WhoDeleted) : ICommand;

public class DeleteFullPurchaseHandler(
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    IMediator mediator) : ICommandHandler<DeleteFullPurchaseCommand>
{
    public async Task<Unit> Handle(DeleteFullPurchaseCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}