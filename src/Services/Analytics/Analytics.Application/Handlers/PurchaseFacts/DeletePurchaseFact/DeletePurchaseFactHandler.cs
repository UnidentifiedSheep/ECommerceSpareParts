using System.Data;
using Abstractions.Interfaces.Services;
using Analytics.Entities;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Handlers.PurchaseFacts.DeletePurchaseFact;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 2, 20)]
public record DeletePurchaseFactCommand(Guid PurchaseId) : ICommand;

public class DeletePurchaseFactHandler(
    IRepository<PurchasesFact, Guid> factRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeletePurchaseFactCommand> logger)
    : ICommandHandler<DeletePurchaseFactCommand>
{
    public async Task<Unit> Handle(DeletePurchaseFactCommand request, CancellationToken cancellationToken)
    {
        var fact = await factRepository.GetById(
            request.PurchaseId,
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