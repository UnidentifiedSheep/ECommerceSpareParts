using System.Net;
using Abstractions.Interfaces.Persistence;
using Analytics.Application.Interfaces.Repositories;
using Analytics.Application.Interfaces.Services.FactSynchronizers;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Application.Models;
using Analytics.Entities;
using Attributes;
using Contracts.Sale;
using Contracts.Sale.Model;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main.Sale;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Services.FactSynchronizers;

public class SaleFactSynchronizer(
    IMainClient mainClient,
    ISaleFactRepository repository,
    IUnitOfWork unitOfWork,
    ITagsService tagsService,
    ILogger<IFactSynchronizer<SalesFact, Guid>> logger
) : ISaleFactSynchronizer
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

    public async Task<SalesFact?> SynchronizeAsync(
        SaleUpdatedEvent saleUpdatedEvent,
        CancellationToken cancellationToken = default)
    {
        return await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(20, 2),
            async () => await ExecuteAsync(saleUpdatedEvent, cancellationToken),
            cancellationToken);
    }

    private async Task<SalesFact?> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var synchronizationStartedAt = DateTime.UtcNow;

        var response = await mainClient.SaleNode.GetFullSale(id, cancellationToken);
        var dbFact = await repository.GetFullSalesFact(id, cancellationToken);

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
        var contents = fromMain.Contents.Select(x => CreateContent(sale.Id, x));

        if (dbFact is null)
        {
            dbFact = SalesFact.Create(
                sale.Id,
                sale.Currency.Id,
                sale.Currency.Id,
                sale.Organization.Id,
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
            sale.Currency.Id,
            sale.Organization.Id,
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

    private async Task<SalesFact?> ExecuteAsync(
        SaleUpdatedEvent saleUpdatedEvent,
        CancellationToken cancellationToken)
    {
        var sale = saleUpdatedEvent.Sale;
        var dbFact = await repository.GetFullSalesFact(sale.Id, cancellationToken);

        if (saleUpdatedEvent.OccurredAt <= dbFact?.ProcessedAt)
        {
            logger.LogWarning(
                "Sale fact Id: {id} upsert skipped, because current record is newer than incoming. " +
                "Last processed at: {lastProcessedAt}. Incoming creation date time: {creationDate}",
                sale.Id,
                dbFact.ProcessedAt,
                saleUpdatedEvent.OccurredAt);

            return dbFact;
        }

        if (sale.State != SaleStateEventModel.Completed)
            return await RemoveFactIfExists(dbFact, cancellationToken);

        var contents = sale.Contents.Select(x => CreateContent(sale.Id, x));

        if (dbFact is null)
        {
            dbFact = SalesFact.Create(
                sale.Id,
                sale.CurrencyId,
                saleUpdatedEvent.BaseCurrencyId,
                sale.OrganizationId,
                sale.SaleDatetime,
                saleUpdatedEvent.OccurredAt,
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
            sale.CurrencyId,
            saleUpdatedEvent.BaseCurrencyId,
            sale.OrganizationId,
            sale.SaleDatetime,
            saleUpdatedEvent.OccurredAt,
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

    private static SaleContent CreateContent(Guid saleId, InternalSaleContent content)
    {
        var details = content.Details.Select(detail =>
            SaleContentDetail.Create(
                detail.Id,
                content.Id,
                detail.Currency.Id,
                detail.BuyPrice,
                detail.BuyPrice,
                detail.Count,
                detail.PurchaseDatetime));

        return SaleContent.Create(
            content.Id,
            saleId,
            content.Product.Id,
            content.Price,
            content.Price,
            content.Count,
            content.Discount,
            details);
    }

    private static SaleContent CreateContent(Guid saleId, SaleContentEventModel content)
    {
        var details = content.Details.Select(detail =>
            SaleContentDetail.Create(
                detail.Id,
                content.Id,
                detail.CurrencyId,
                detail.BuyPrice,
                detail.BuyPriceInBaseCurrency,
                detail.Count,
                detail.PurchaseDatetime));

        return SaleContent.Create(
            content.Id,
            saleId,
            content.ProductId,
            content.Price,
            content.PriceInBaseCurrency,
            content.Count,
            content.Discount,
            details);
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
