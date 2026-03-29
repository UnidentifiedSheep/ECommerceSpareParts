using System.Data;
using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Analytics.Abstractions.Exceptions.PurchaseFacts;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities;
using Application.Common.Interfaces;
using Attributes;
using MediatR;

namespace Analytics.Application.Handlers.PurchaseFacts.DeletePurchaseFact;

[Transactional(IsolationLevel.ReadCommitted, 2, 20)]
public record DeletePurchaseFactCommand(string PurchaseId) : ICommand;

public class DeletePurchaseFactHandler(
    IPurchaseFactRepository purchaseFactRepository,
    IUnitOfWork unitOfWork) 
    : ICommandHandler<DeletePurchaseFactCommand>
{
    public async Task<Unit> Handle(DeletePurchaseFactCommand request, CancellationToken cancellationToken)
    {
        var fact = await purchaseFactRepository.GetFact(
            new QueryOptions<PurchasesFact, string>() { Data = request.PurchaseId }
                .WithForUpdate()
                .WithTracking(),
            cancellationToken) ?? throw new PurchaseFactNotFoundException(request.PurchaseId);
        
        unitOfWork.Remove(fact);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}