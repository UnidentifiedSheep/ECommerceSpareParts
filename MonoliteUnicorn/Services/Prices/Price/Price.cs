using System.Globalization;
using Core.Redis;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Prices.PriceGenerator;
using StackExchange.Redis;

namespace MonoliteUnicorn.Services.Prices.Price;

public class Price(DContext context, ILogger<Price> logger) : IPrice
{
    public Task<double> GetPrice(int articleId, int currencyId, string? buyerId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public async Task<Dictionary<int, double>> GetPrices(IEnumerable<int> articleIds, int currencyId, string? buyerId, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<int, double>();
        var redis = Redis.GetRedis();
        var userDiscount = await GetUserDiscount(buyerId, cancellationToken);
        var prices = await GetOrRecalculateUsablePrice(articleIds, redis, cancellationToken);
        foreach (var (articleId, usablePrice) in prices)
        {
            var converted = CurrencyConverter.ConvertTo(usablePrice, Global.UsdId, currencyId);
            var sellPrice = PriceGenerator.PriceGenerator.GetSellPrice(converted, userDiscount, currencyId);
            results[articleId] = sellPrice;
        }
        return results;
    }

    public async Task<Dictionary<int, DetailedPriceModel>> GetDetailedPrices(IEnumerable<int> articleIds, int currencyId, string? buyerId, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<int, DetailedPriceModel>();
        var redis = Redis.GetRedis();
        var userDiscount = await GetUserDiscount(buyerId, cancellationToken);
        var prices = await GetOrRecalculateUsablePrice(articleIds, redis, cancellationToken);
        foreach (var (articleId, usablePrice) in prices)
        {
            var converted = CurrencyConverter.ConvertTo(usablePrice, Global.UsdId, currencyId);
            results[articleId] = new DetailedPriceModel
            {
                Id = articleId,
                PriceInUsd = usablePrice,
                MinPrice = PriceGenerator.PriceGenerator.GetSellPriceWithMinimalMarkUp(converted),
                RecommendedPrice = PriceGenerator.PriceGenerator.GetSellPrice(converted, 0, currencyId),
                RecommendedPriceWithDiscount = PriceGenerator.PriceGenerator.GetSellPrice(converted, userDiscount, currencyId)
            };
        }
        return results;
    }

    public async Task RecalculateUsablePrice(IEnumerable<(int ArticleId, decimal BuyPriceInUsd)> values, CancellationToken cancellationToken = default)
    {
        var redis = Redis.GetRedis();
        var results = new List<KeyValuePair<RedisKey, RedisValue>>();

        var avgPrices = values.GroupBy(v => v.ArticleId)
            .Select(g => (ArticleId: g.Key, AvgPrice: g.Average(x => x.BuyPriceInUsd)))
            .ToList();

        foreach (var (articleId, avgPrice) in avgPrices)
        {
            var highest = await GetHighestBuyPrices(articleId, cancellationToken);
            highest.Add(avgPrice);
            var avg = Math.Round(highest.Average(), 2);
            results.Add(new($"usablePrice:{articleId}", avg.ToString(CultureInfo.InvariantCulture)));
        }

        await redis.StringSetAsync(results.ToArray());
    }

    private async Task<decimal> RecalculateUsablePrice(int articleId, CancellationToken cancellationToken = default)
    {
        var redis = Redis.GetRedis();
        var highest = await GetHighestBuyPrices(articleId, cancellationToken);
        var avg = Math.Round(highest.Average(), 2);
        await redis.StringSetAsync($"usablePrice:{articleId}", avg.ToString(CultureInfo.InvariantCulture));
        return avg;
    }

    private async Task<List<decimal>> GetHighestBuyPrices(int articleId, CancellationToken cancellationToken)
        => await context.StorageContents.AsNoTracking()
            .Where(x => x.ArticleId == articleId && x.Count > 0 && x.Status == nameof(StorageContentStatus.Ok))
            .OrderByDescending(x => x.BuyPriceInUsd)
            .Select(x => x.BuyPriceInUsd)
            .Take(5)
            .ToListAsync(cancellationToken);

    private async Task<double?> GetOrRecalculateUsablePrice(int articleId, IDatabase redis, CancellationToken cancellationToken)
    {
        var redisValue = await redis.StringGetAsync($"usablePrice:{articleId}");
        if (!redisValue.HasValue)
        {
            var exists = await context.StorageContents.AsNoTracking()
                .AnyAsync(x => x.ArticleId == articleId && x.Count > 0, cancellationToken);

            if (!exists)
            {
                await TriggerPriceRecalculation(articleId, redis);
                return null;
            }

            var price = await RecalculateUsablePrice(articleId, cancellationToken);
            redisValue = price.ToString(CultureInfo.InvariantCulture);
        }

        if (!double.TryParse(redisValue, CultureInfo.InvariantCulture, out var usablePrice)) return null;
        return usablePrice;
    }
    
    private async Task<Dictionary<int, double>> GetOrRecalculateUsablePrice(IEnumerable<int> articleIds, IDatabase redis, CancellationToken cancellationToken)
    {
        var results = new Dictionary<int, double>();
        var articles = articleIds.DistinctBy(x => x).ToList();
        var usablePrices = await redis.StringGetAsync(articles.Select(x => new RedisKey($"usablePrice:{x}")).ToArray());
        for (int i = 0; i < usablePrices.Length; i++)
        {
            var articleId = articles[i];
            var redisValue = usablePrices[i];
            if (!redisValue.HasValue)
            {
                var exists = await context.StorageContents.AsNoTracking()
                    .AnyAsync(x => x.ArticleId == articleId && x.Count > 0, cancellationToken);

                if (!exists)
                {
                    await TriggerPriceRecalculation(articleId, redis);
                    continue;
                }

                var price = await RecalculateUsablePrice(articleId, cancellationToken);
                redisValue = price.ToString(CultureInfo.InvariantCulture);
            }
            if (!double.TryParse(redisValue, CultureInfo.InvariantCulture, out var usablePrice) || usablePrice <= 0) continue;
            results[articleId] = usablePrice;
        }
        return results;
    }

    private async Task TriggerPriceRecalculation(int articleId, IDatabase redis)
    {
        var updateKey = $"articleUpdatePriceTime:{articleId}";
        var updateTime = await redis.StringGetAsync(updateKey);

        if (!updateTime.HasValue)
        {
            _ = Task.Run(async () => await RecalculatePrice(articleId));
            await redis.StringSetAsync(updateKey, DateTime.Now.ToString(CultureInfo.InvariantCulture), TimeSpan.FromDays(1));
        }
    }

    private async Task<double> GetUserDiscount(string? buyerId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(buyerId)) return 0;
        var redis = Redis.GetRedis();

        var redisValue = await redis.StringGetAsync($"userDiscount:{buyerId}");
        if (redisValue.HasValue && double.TryParse(redisValue, out var value)) return value;

        var discount = await context.UserDiscounts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == buyerId, cancellationToken: cancellationToken);

        return (double?)discount?.Discount ?? 0;
    }

    private async Task RecalculatePrice(int articleId)
    {
        try
        {
            // TODO: логика запроса к поставщикам
            return;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, $"Error recalculating price for Article ID {articleId}");
        }
    }
}
