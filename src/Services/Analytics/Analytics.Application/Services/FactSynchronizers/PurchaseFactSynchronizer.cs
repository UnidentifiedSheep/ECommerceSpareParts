using System.Net;
using Abstractions.Interfaces.Persistence;
using Analytics.Application.Interfaces.Services.FactSynchronizers;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Application.Models;
using Analytics.Entities;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Services.FactSynchronizers;

public class PurchaseFactSynchronizer(
    IMainClient mainClient,
    IRepository<PurchasesFact, Guid> repository,
    IUnitOfWork unitOfWork,
    ITagsService tagsService,
    ILogger<IFactSynchronizer<PurchasesFact, Guid>> logger) : IFactSynchronizer<PurchasesFact, Guid>
{
    public async Task<PurchasesFact?> SynchronizeAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(20, 2),
            async () => await ExecuteAsync(id, cancellationToken),
            cancellationToken);
    }

    private async Task<PurchasesFact?> ExecuteAsync(
        Guid id, 
        CancellationToken cancellationToken)
    {
        var synchronizationStartedAt = DateTime.UtcNow;
        
        var response = await mainClient.PurchaseNode.GetFullPurchase(id, cancellationToken);
        var dbFact = await repository.FirstOrDefaultAsync(
            Criteria<PurchasesFact>
                .New()
                .Where(x => x.Id == id)
                .Include(x => x.PurchaseContents)
                .Track()
                .Build(),
            cancellationToken);
         
        if (synchronizationStartedAt <= dbFact?.ProcessedAt)
        {
            logger.LogWarning(
                "Purchase fact Id: {id} upsert skipped, because current record is newer than incoming." +
                "Last processed at: {lastProcessedAt}. Incoming creation date time: {creationDate}",
                id,
                dbFact.ProcessedAt,
                synchronizationStartedAt);

            return dbFact;
        }

        if (!response.Success)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
                return await RemoveFactIfExists(dbFact, cancellationToken);

            throw new InvalidOperationException(
                $"Unable to synchronize purchase fact {id}. " +
                $"Main service returned {response.StatusCode}: {response.Error}");
        }

        var fromMain = response.ValueOrThrow;

        var purchase = fromMain.Purchase;
        var contents = fromMain.Contents.Select(x =>
            PurchaseContent.Create(
                x.Id,
                purchase.Id,
                x.Product.Id,
                x.Price,
                x.Count));

        if (dbFact is null)
        {
            dbFact = PurchasesFact.Create(
                purchase.Id,
                purchase.Currency.Id,
                purchase.Supplier.Id,
                purchase.PurchaseDatetime,
                synchronizationStartedAt,
                contents);

            await unitOfWork.AddAsync(dbFact, cancellationToken);
            await tagsService.UpdateTags(
                new TagUpdateContext<PurchasesFact>
                {
                    NewFactDatetime = dbFact.CreatedAt
                },
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return dbFact;
        }

        var previousFactDatetime = dbFact.CreatedAt;

        dbFact.Update(
            purchase.Currency.Id,
            purchase.Supplier.Id,
            purchase.PurchaseDatetime,
            synchronizationStartedAt,
            contents);

        await tagsService.UpdateTags(
            new TagUpdateContext<PurchasesFact>
            {
                NewFactDatetime = dbFact.CreatedAt,
                PreviousFactDatetime = previousFactDatetime
            },
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return dbFact;
    }

    private async Task<PurchasesFact?> RemoveFactIfExists(
        PurchasesFact? dbFact,
        CancellationToken cancellationToken)
    {
        if (dbFact is not null)
        {
            await tagsService.UpdateTags(
                new TagUpdateContext<PurchasesFact>
                {
                    NewFactDatetime = dbFact.CreatedAt
                },
                cancellationToken);

            unitOfWork.Remove(dbFact);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return null;
    }
}
