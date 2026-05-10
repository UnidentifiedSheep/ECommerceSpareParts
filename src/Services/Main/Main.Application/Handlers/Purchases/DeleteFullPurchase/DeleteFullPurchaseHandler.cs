using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using MassTransit;
using MediatR;

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