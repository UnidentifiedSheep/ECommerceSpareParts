using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Analytics.Abstractions.Dtos.PurchaseFact;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities;
using Application.Common.Interfaces;
using Attributes;
using Mapster;
using MediatR;

namespace Analytics.Application.Handlers.PurchaseFacts.UpsertPurchaseFact;

[Transactional]
public record UpsertPurchaseFactCommand(PurchaseFactUpsertDto PurchaseFact) : ICommand;

public class UpsertPurchaseFactHandler(
    IPurchaseFactRepository factRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpsertPurchaseFactCommand>
{
    public async Task<Unit> Handle(UpsertPurchaseFactCommand request, CancellationToken cancellationToken)
    {
        var newFact = request.PurchaseFact;
        var dbFact = await factRepository.GetFact(
            newFact.Id, 
            QueryPresets.TrackForUpdate, 
            cancellationToken);

        if (dbFact == null)
        {
            dbFact = new PurchasesFact();
            await unitOfWork.AddAsync(dbFact, cancellationToken);
        }

        newFact.Adapt(dbFact);
        
        dbFact.TotalSum = dbFact.PurchaseContents.Sum(x => x.Count * x.Price);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}