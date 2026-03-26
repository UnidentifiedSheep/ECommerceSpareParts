using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Analytics.Abstractions.Dtos.PurchaseFact;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities;
using Application.Common.Interfaces;
using Attributes;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Handlers.PurchaseFacts.UpsertPurchaseFact;

[Transactional]
public record UpsertPurchaseFactCommand(PurchaseFactUpsertDto PurchaseFact) : ICommand;

public class UpsertPurchaseFactHandler(
    IPurchaseFactRepository factRepository,
    ILogger<UpsertPurchaseFactCommand> logger,
    IUnitOfWork unitOfWork) : ICommandHandler<UpsertPurchaseFactCommand>
{
    public async Task<Unit> Handle(UpsertPurchaseFactCommand request, CancellationToken cancellationToken)
    {
        var newFact = request.PurchaseFact;
        var dbFact = await factRepository.GetFact(
            newFact.Id,
            QueryPresets.TrackForUpdate,
            cancellationToken);

        var shouldBeAdded = false;

        if (dbFact == null)
        {
            dbFact = new PurchasesFact();
            shouldBeAdded = true;
        }
        else if (newFact.LastUpdatedAt <= dbFact.ProcessedAt)
        {
            logger.LogWarning(
                "Purchase fact Id: {id} upsert skipped, because current record is newer than incoming." +
                "Last processed at: {lastProcessedAt}. Incoming creation date time: {creationDate}",
                newFact.Id,
                dbFact.ProcessedAt,
                newFact.LastUpdatedAt);

            return Unit.Value;
        }

        //update fields.
        newFact.Adapt(dbFact);
        var sumAccumulator = 0m;
        var existingContents = dbFact.PurchaseContents.ToDictionary(x => x.Id);
        var toRemove = new Dictionary<int, PurchaseContent>(existingContents);

        foreach (var newContent in newFact.Content)
        {
            toRemove.Remove(newContent.Id);
            sumAccumulator += newContent.Count * newContent.Price;
            if (existingContents.TryGetValue(newContent.Id, out var existingContent))
                newContent.Adapt(existingContent);
            else
                dbFact.PurchaseContents.Add(newContent.Adapt<PurchaseContent>());
        }

        dbFact.TotalSum = sumAccumulator;

        if (toRemove.Count != 0)
            foreach (var item in toRemove.Values)
                dbFact.PurchaseContents.Remove(item);


        //add to db if needed
        if (shouldBeAdded)
            await unitOfWork.AddAsync(dbFact, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}