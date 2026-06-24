using System.Net;
using Abstractions.Interfaces.Persistence;
using Analytics.Application.Interfaces.Services.FactSynchronizers;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Application.Models;
using Analytics.Entities;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main.Sale;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Services.FactSynchronizers;

public class SaleFactSynchronizer(
    IMainClient mainClient,
    IRepository<SalesFact, Guid> repository,
    IUnitOfWork unitOfWork,
    ITagsService tagsService,
    ILogger<IFactSynchronizer<SalesFact, Guid>> logger) : IFactSynchronizer<SalesFact, Guid>
{
    public async Task<SalesFact?> SynchronizeAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(20, 2),
            async () => await ExecuteAsync(id, cancellationToken),
            cancellationToken);
    }

    private async Task<SalesFact?> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var synchronizationStartedAt = DateTime.UtcNow;

        var response = await mainClient.SaleNode.GetFullSale(id, cancellationToken);
        var dbFact = await repository.FirstOrDefaultAsync(
            Criteria<SalesFact>
                .New()
                .Where(x => x.Id == id)
                .Include(x => x.SaleContents)
                .Track()
                .Build(),
            cancellationToken);

        if (synchronizationStartedAt <= dbFact?.ProcessedAt)
        {
            logger.LogWarning(
                "Sale fact Id: {id} upsert skipped, because current record is newer than incoming. " +
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
                $"Unable to synchronize sale fact {id}. " +
                $"Main service returned {response.StatusCode}: {response.Error}");
        }

        var fromMain = response.ValueOrThrow;

        if (fromMain.Sale.State != InternalSaleState.Completed)
            return await RemoveFactIfExists(dbFact, cancellationToken);

        var sale = fromMain.Sale;
        var contents = fromMain.Contents.Select(x =>
            SaleContent.Create(
                x.Id,
                sale.Id,
                x.Product.Id,
                x.Price,
                x.Count,
                x.Discount));

        if (dbFact is null)
        {
            dbFact = SalesFact.Create(
                sale.Id,
                sale.Currency.Id,
                sale.Buyer.Id,
                sale.SaleDatetime,
                synchronizationStartedAt,
                contents);

            await unitOfWork.AddAsync(dbFact, cancellationToken);
            await tagsService.UpdateTags(
                new TagUpdateContext<SalesFact>
                {
                    NewFactDatetime = dbFact.CreatedAt
                },
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return dbFact;
        }

        var previousFactDatetime = dbFact.CreatedAt;

        dbFact.Update(
            sale.Currency.Id,
            sale.Buyer.Id,
            sale.SaleDatetime,
            synchronizationStartedAt,
            contents);

        await tagsService.UpdateTags(
            new TagUpdateContext<SalesFact>
            {
                NewFactDatetime = dbFact.CreatedAt,
                PreviousFactDatetime = previousFactDatetime
            },
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return dbFact;
    }

    private async Task<SalesFact?> RemoveFactIfExists(
        SalesFact? dbFact,
        CancellationToken cancellationToken)
    {
        if (dbFact is not null)
        {
            await tagsService.UpdateTags(
                new TagUpdateContext<SalesFact>
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
