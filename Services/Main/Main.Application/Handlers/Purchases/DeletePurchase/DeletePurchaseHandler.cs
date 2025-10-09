using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Purchase;
using MediatR;

namespace Main.Application.Handlers.Purchases.DeletePurchase;

[Transactional]
public record DeletePurchaseCommand(string PurchaseId) : ICommand<Unit>;

public class DeletePurchaseHandler(IPurchaseRepository purchaseRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePurchaseCommand, Unit>
{
    public async Task<Unit> Handle(DeletePurchaseCommand request, CancellationToken cancellationToken)
    {
        var purchase = await purchaseRepository.GetPurchaseForUpdate(request.PurchaseId, true, cancellationToken)
                       ?? throw new PurchaseNotFoundException(request.PurchaseId);
        unitOfWork.Remove(purchase);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}