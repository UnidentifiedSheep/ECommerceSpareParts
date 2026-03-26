using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Exceptions.Purchase;
using Main.Abstractions.Interfaces.DbRepositories;
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
            request.PurchaseId,
            QueryPresets.TrackForUpdate,
            cancellationToken) ?? throw new PurchaseNotFoundException(request.PurchaseId);
        unitOfWork.Remove(purchase);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}