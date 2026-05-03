using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Entities;
using Main.Entities.Exceptions.Purchase;
using Main.Entities.Purchase;
using MediatR;

namespace Main.Application.Handlers.Purchases.DeletePurchase;

[Transactional]
public record DeletePurchaseCommand(string PurchaseId) : ICommand<Unit>;

public class DeletePurchaseHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePurchaseCommand, Unit>
{
    public async Task<Unit> Handle(DeletePurchaseCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}