using System.Data;
using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities;
using Application.Common.Interfaces;
using Attributes;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Handlers.PurchaseFacts.DeletePurchaseFact;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 2, 20)]
public record DeletePurchaseFactCommand(string PurchaseId) : ICommand;

public class DeletePurchaseFactHandler(
    IPurchaseFactRepository purchaseFactRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeletePurchaseFactCommand> logger) 
    : ICommandHandler<DeletePurchaseFactCommand>
{
    public async Task<Unit> Handle(DeletePurchaseFactCommand request, CancellationToken cancellationToken)
    {
        var fact = await purchaseFactRepository.GetFact(
            new QueryOptions<PurchasesFact, string>() { Data = request.PurchaseId }
                .WithForUpdate()
                .WithTracking(),
            cancellationToken);

        if (fact == null)
        {
            logger.LogWarning("Unable to to delete purchase fact {factId}, because it doesn't exist", 
                request.PurchaseId);
            return Unit.Value;
        }
        
        unitOfWork.Remove(fact);
        return Unit.Value;
    }
}