using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Exceptions.Purchase;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using MediatR;

namespace Main.Application.Handlers.Purchases.DeletePurchase;

[Transactional]
public record DeletePurchaseCommand(string PurchaseId) : ICommand<Unit>;

public class DeletePurchaseHandler(IPurchaseRepository purchaseRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePurchaseCommand, Unit>
{
    public async Task<Unit> Handle(DeletePurchaseCommand request, CancellationToken cancellationToken)
    {
        var purchase = await purchaseRepository.GetPurchase(
            new QueryOptions<Purchase, string>() { Data = request.PurchaseId }
                .WithTracking()
                .WithForUpdate(),
            cancellationToken) ?? throw new PurchaseNotFoundException(request.PurchaseId);
        unitOfWork.Remove(purchase);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}