using Core.TransactionBuilder;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Prices.PriceGenerator;

namespace MonoliteUnicorn.Services.BuySellPriceService;

public class BuySellPriceService(DContext context) : IBuySellPriceService
{
    public async Task AddBuySellPrices(IEnumerable<StorageContent> storageContents, IEnumerable<SaleContent> saleContents,
        int currencyId, CancellationToken cancellationToken = default)
    {
        await context.WithDefaultTransactionSettings("normal")
            .ExecuteWithTransaction(async () =>
            {
                var articleBuyPrices = storageContents
                    .GroupBy(x => x.ArticleId, x => x.BuyPriceInUsd)
                    .ToDictionary(x => x.Key, x => x.Average());
                foreach (var content in saleContents)
                {
                    var avrgBuyPrice = articleBuyPrices[content.ArticleId];
                    var buySellPrices = new BuySellPrice
                    {
                        BuyPrice = Math.Round(CurrencyConverter.ConvertTo(avrgBuyPrice, Global.UsdId, currencyId), 2),
                        SellPrice = Math.Round(content.Price, 2),
                        CurrencyId = currencyId,
                        SaleContentId = content.Id
                    };
                    await context.BuySellPrices.AddAsync(buySellPrices, cancellationToken);
                }

                await context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
    }

    public async Task EditBuySellPrices(IEnumerable<SaleContent> saleContents, int currencyId,
        CancellationToken cancellationToken = default)
    {
        await context.WithDefaultTransactionSettings("normal")
            .ExecuteWithTransaction(async () =>
            {
                var saleContentsDict = saleContents.ToDictionary(x => x.Id);
                var buySellPrices = await context.BuySellPrices
                    .FromSql($"""
                              select * from buy_sell_prices 
                              where sale_content_id = ANY({saleContentsDict.Keys})
                              for update
                              """)
                    .ToListAsync(cancellationToken);
                foreach (var bsPrice in buySellPrices)
                {
                    var sContent = saleContentsDict[bsPrice.SaleContentId];
                    if (Math.Round(sContent.Price, 2) <= 0)
                        throw new ArgumentException($"Цена для артикула {sContent.ArticleId} должна быть положительной");
                    if (bsPrice.CurrencyId == currencyId && bsPrice.SellPrice == sContent.Price) continue;
                    bsPrice.SellPrice =
                        Math.Round(CurrencyConverter.ConvertTo(sContent.Price, bsPrice.CurrencyId, currencyId), 2);
                    bsPrice.CurrencyId = currencyId;
                }

                await context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
    }
}