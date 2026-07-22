using System.Linq.Expressions;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Contracts.Sale;
using Contracts.Sale.Model;
using LinqKit;
using Main.Application.Interfaces.Services.Event;
using Main.Entities.Exceptions;
using Main.Entities.Sale;
using Main.Entities.Settings;
using Main.Enums;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Services.Event;

public sealed class SaleEventService(
    IReadRepository<Sale, Guid> repository,
    IIntegrationEventScope integrationEventScope,
    ICurrencyConverter currencyConverter,
    ISettingsService settingsService
) : ISaleEventService
{
    public static readonly Expression<Func<Sale, SaleEventModel>> ToSaleEvent =
        x => new SaleEventModel
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            UserId = x.UserId,
            Comment = x.Comment,
            CurrencyId = x.CurrencyId,
            RowVersion = x.RowVersion,
            SaleDatetime = x.SaleDatetime,
            State = x.State == SaleState.Completed ? SaleStateEventModel.Completed :
                x.State == SaleState.Deleted ? SaleStateEventModel.Deleted :
                SaleStateEventModel.Draft,
            Storage = x.StorageName,
            TotalSum = x.Transaction.Amount,
            TransactionId = x.TransactionId,
            Contents = x.Contents.Select(z => ToSaleContentEvent.Invoke(z)).ToList()
        };

    private static readonly Expression<Func<SaleContent, SaleContentEventModel>> ToSaleContentEvent =
        x => new SaleContentEventModel
        {
            Comment = x.Comment,
            Count = x.Count,
            Discount = x.Discount,
            ProductId = x.ProductId,
            Id = x.Id,
            Price = x.Price,
            PriceInBaseCurrency = 0,
            Details = x.Details.Select(z => ToSaleContentDetailEvent.Invoke(z)).ToList()
        };

    private static readonly Expression<Func<SaleContentDetail, SaleContentDetailEventModel>>
        ToSaleContentDetailEvent =
            x => new SaleContentDetailEventModel
            {
                BuyPrice = x.BuyPrice,
                BuyPriceInBaseCurrency = x.StorageContent.BuyPriceInBaseCurrency,
                Count = x.Count,
                CurrencyId = x.CurrencyId,
                PurchaseDatetime = x.PurchaseDatetime,
                SaleContentId = x.SaleContentId,
                StorageContentId = x.StorageContentId,
                Id = x.Id
            };

    public async Task NotifyUpdated(
        Guid id,
        CancellationToken cancellationToken)
    {
        var baseCurrencyId = (await settingsService.GetOrDefault<CurrencySetting>(cancellationToken))
            .Data.BaseCurrencyId;

        var fromDb = await repository.Query
                         .Where(x => x.Id == id)
                         .AsExpandable()
                         .Select(ToSaleEvent)
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new SaleNotFoundException(id);

        var newContents = new List<SaleContentEventModel>();
        foreach (var content in fromDb.Contents)
            newContents.Add(
                content with
                {
                    PriceInBaseCurrency = await currencyConverter.ConvertToBaseAsync(
                        content.Price,
                        fromDb.CurrencyId,
                        cancellationToken)
                });

        fromDb = fromDb with { Contents = newContents };

        integrationEventScope.Add(
            new SaleUpdatedEvent
            {
                BaseCurrencyId = baseCurrencyId,
                OccurredAt = DateTime.UtcNow,
                Sale = fromDb
            });
    }
}
