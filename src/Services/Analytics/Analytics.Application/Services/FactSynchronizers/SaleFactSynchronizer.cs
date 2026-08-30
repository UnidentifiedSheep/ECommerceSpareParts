using Abstractions.Interfaces.Persistence;
using Analytics.Application.Interfaces.Repositories;
using Analytics.Application.Interfaces.Services.FactSynchronizers;
using Analytics.Entities;
using Attributes;
using Contracts.Sale;
using Contracts.Sale.Model;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Services.FactSynchronizers;

public class SaleFactSynchronizer(
    ISaleFactRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<ISaleFactSynchronizer> logger
) : ISaleFactSynchronizer
{
    public async Task<SalesFact?> SynchronizeAsync(
        SaleUpdatedEvent saleUpdatedEvent,
        CancellationToken cancellationToken = default)
    {
        return await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.Serializable(20, 2),
            async () => await ExecuteAsync(saleUpdatedEvent, cancellationToken),
            cancellationToken);
    }

    public async Task<SalesFact?> SynchronizeAsync(
        SaleDeletedEvent saleDeletedEvent,
        CancellationToken cancellationToken = default)
    {
        return await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.Serializable(20, 2),
            async () => await ExecuteAsync(saleDeletedEvent, cancellationToken),
            cancellationToken);
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
            return await MarkDeletedAsync(
                sale.Id,
                saleUpdatedEvent.OccurredAt,
                dbFact,
                cancellationToken);

        var contents = sale.Contents.Select(x => CreateContent(sale.Id, x));

        if (dbFact is null)
        {
            dbFact = SalesFact.Create(
                sale.Id,
                sale.CurrencyId,
                saleUpdatedEvent.BaseCurrencyId,
                sale.OrganizationId,
                sale.UserId,
                sale.SaleDatetime,
                saleUpdatedEvent.OccurredAt,
                contents);

            await unitOfWork.AddAsync(dbFact, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return dbFact;
        }

        dbFact.Update(
            sale.CurrencyId,
            saleUpdatedEvent.BaseCurrencyId,
            sale.OrganizationId,
            sale.UserId,
            sale.SaleDatetime,
            saleUpdatedEvent.OccurredAt,
            contents);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return dbFact;
    }

    private async Task<SalesFact?> ExecuteAsync(
        SaleDeletedEvent saleDeletedEvent,
        CancellationToken cancellationToken)
    {
        var occurredAt = saleDeletedEvent.OccurredAt == default
            ? DateTime.UtcNow
            : saleDeletedEvent.OccurredAt;
        var dbFact = await repository.GetFullSalesFact(
            saleDeletedEvent.SaleId,
            cancellationToken);

        if (occurredAt <= dbFact?.ProcessedAt)
        {
            logger.LogWarning(
                "Sale fact Id: {id} delete skipped, because current record is newer than incoming. " +
                "Last processed at: {lastProcessedAt}. Incoming creation date time: {creationDate}",
                saleDeletedEvent.SaleId,
                dbFact.ProcessedAt,
                occurredAt);

            return dbFact;
        }

        return await MarkDeletedAsync(
            saleDeletedEvent.SaleId,
            occurredAt,
            dbFact,
            cancellationToken);
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

    private async Task<SalesFact?> MarkDeletedAsync(
        Guid id,
        DateTime processedAt,
        SalesFact? dbFact,
        CancellationToken cancellationToken)
    {
        if (dbFact is null)
        {
            dbFact = SalesFact.CreateDeleted(id, processedAt);
            await unitOfWork.AddAsync(dbFact, cancellationToken);
        }
        else
        {
            dbFact.MarkDeleted(processedAt);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return null;
    }
}
