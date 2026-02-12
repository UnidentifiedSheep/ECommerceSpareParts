using Abstractions.Interfaces.Currency;
using Abstractions.Interfaces.Services;
using Analytics.Core.Entities;
using Analytics.Core.Interfaces.DbRepositories;
using Analytics.Core.Interfaces.Services;
using Analytics.Core.Static;
using Contracts.Models.Sale;

namespace Analytics.Application.Services;

public class SellInfoService(IUnitOfWork unitOfWork, ISellInfoRepository sellInfoRepository,
    ICurrencyConverter currencyConverter) : ISellInfoService
{
    public async Task<IEnumerable<SellInfo>> RemoveSellInfos(IEnumerable<int> saleContentIds,
        CancellationToken cancellationToken = default)
    {
        var toRemoveSet = new HashSet<int>(saleContentIds);
        if (toRemoveSet.Count == 0) return [];
        
        var neededToBeRemoved = (await sellInfoRepository
            .GetSellInfosList(x => toRemoveSet.Contains(x.SellContentId), true, cancellationToken)).ToList();
        unitOfWork.RemoveRange(neededToBeRemoved);
        return neededToBeRemoved;
    }

    public async Task<IEnumerable<SellInfo>> CreateSellInfos(Sale sell, CancellationToken cancellationToken = default)
    {
        var sellInfos = sell.SaleContents.Select(content => CreateSellInfo(content, sell.CurrencyId, 
            sell.MainStorageName, sell.SaleDatetime)).ToList();
        await unitOfWork.AddRangeAsync(sellInfos, cancellationToken);
        return sellInfos;
    }

    public async Task<IEnumerable<SellInfo>> EditSellInfos(Sale sell, CancellationToken cancellationToken = default)
    {
        var sellInfos = new List<SellInfo>();
        var sellContentIds = sell.SaleContents.Select(x => x.Id).ToHashSet();
        var saleContents = (await sellInfoRepository.GetSellInfosList(x => sellContentIds.Contains(x.SellContentId),
            true, cancellationToken)).ToDictionary(x => x.SellContentId);
        var toAdd = new List<SellInfo>();
        foreach (SaleContent item in sell.SaleContents)
        {
            if (saleContents.TryGetValue(item.Id, out var existingContent))
            {
                var avrgBuyPrice = GetAvrgConvertedBuyPrice(item.Details, sell.CurrencyId);
                
                existingContent.BuyPrices = avrgBuyPrice;
                existingContent.BuyCurrencyId = existingContent.SellCurrencyId = sell.CurrencyId;
                existingContent.StorageName = sell.MainStorageName;
                existingContent.ArticleId = item.ArticleId;
                existingContent.SellPrice = item.Price;
                existingContent.SellDate = sell.SaleDatetime;
                existingContent.Markup = Calculate.Markup(avrgBuyPrice, item.Price);
                
                sellInfos.Add(existingContent);
            }
            else
            {
                var sellInfo = CreateSellInfo(item, sell.CurrencyId, sell.MainStorageName, sell.SaleDatetime);
                toAdd.Add(sellInfo);
                sellInfos.Add(sellInfo);
            }
        }
        
        await unitOfWork.AddRangeAsync(toAdd, cancellationToken);
        return sellInfos;
    }

    private decimal GetAvrgConvertedBuyPrice(IEnumerable<SaleContentDetail> details, int toCurrency)
    {
        return details
            .Select(x => currencyConverter
                .ConvertTo(x.BuyPrice, x.CurrencyId, toCurrency))
            .Average();
    }

    private SellInfo CreateSellInfo(SaleContent content, int currencyId, string storageName, DateTime sellDateTime)
    {
        var avrgBuyPrice = GetAvrgConvertedBuyPrice(content.Details, currencyId);
        return new SellInfo
        {
            ArticleId = content.ArticleId,
            BuyCurrencyId = currencyId,
            BuyPrices = avrgBuyPrice,
            StorageName = storageName,
            SellPrice = content.Price,
            SellCurrencyId = currencyId,
            SellContentId = content.Id,
            Markup = Calculate.Markup(avrgBuyPrice, content.Price),
            SellDate = sellDateTime
        };
    }
}