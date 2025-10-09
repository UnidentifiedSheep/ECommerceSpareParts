using Core.Interfaces.CacheRepositories;
using StackExchange.Redis;

namespace Redis.Repositories;

public class RedisArticlePricesRepository : IRedisArticlePriceRepository
{
    private readonly IDatabase _redis;
    private readonly TimeSpan? _ttl;

    public RedisArticlePricesRepository(IDatabase redis, TimeSpan? ttl = null)
    {
        _redis = redis;
        _ttl = ttl;
    }

    public async Task<Dictionary<int, double?>> GetUsablePricesAsync(IEnumerable<int> articleIds)
    {
        var ids = articleIds.DistinctBy(x => x).ToArray();
        var usablePrices = await _redis.StringGetAsync(ids.Select(x => new RedisKey(GetUsablePriceKey(x))).ToArray());
        var result = new Dictionary<int, double?>();
        for (var i = 0; i < usablePrices.Length; i++)
        {
            var id = ids[i];
            var redisValue = usablePrices[i];
            double? value = null;
            if (redisValue.HasValue && double.TryParse(redisValue, Global.Culture, out var price))
                value = price;
            result.Add(id, value);
        }

        return result;
    }

    public async Task SetUsablePricesAsync(Dictionary<int, double> prices)
    {
        var batch = _redis.CreateBatch();
        var tasks = new List<Task>();
        foreach (var (articleId, price) in prices)
        {
            if (price <= 0) continue;
            tasks.Add(batch.StringSetAsync(GetUsablePriceKey(articleId), price, _ttl));
        }

        batch.Execute();
        await Task.WhenAll(tasks);
    }

    public async Task SetUsablePriceAsync(int articleId, double price)
    {
        if (price <= 0) return;
        await _redis.StringSetAsync(GetUsablePriceKey(articleId), price, _ttl);
    }

    public async Task<DateTime?> GetPriceUpdateTimeAsync(int articleId)
    {
        var key = GetPriceUpdateKey(articleId);
        var value = await _redis.StringGetAsync(key);
        if (value.HasValue && DateTime.TryParse(value, Global.Culture, out var date))
            return date;
        return null;
    }

    public async Task<Dictionary<int, DateTime?>> GetPriceUpdateTimeAsync(IEnumerable<int> articleIds)
    {
        var ids = articleIds.DistinctBy(x => x).ToArray();
        var keys = ids
            .Select(x => new RedisKey(GetPriceUpdateKey(x)))
            .ToArray();
        var result = new Dictionary<int, DateTime?>();
        var redisResult = await _redis.StringGetAsync(keys);
        for (var i = 0; i < redisResult.Length; i++)
        {
            var id = ids[i];
            var redisValue = redisResult[i];
            if (redisValue.HasValue && DateTime.TryParse(redisValue, Global.Culture, out var date))
            {
                result.Add(id, date);
                continue;
            }

            result.Add(id, null);
        }

        return result;
    }

    public async Task SetPriceUpdateTimeAsync(int articleId, DateTime updateTime)
    {
        await _redis.StringSetAsync(GetPriceUpdateKey(articleId), updateTime.ToString(Global.Culture), _ttl);
    }

    public async Task SetPriceUpdateTimeAsync(IEnumerable<int> ids, DateTime updateTime)
    {
        var batch = _redis.CreateBatch();
        var tasks = new List<Task>();
        ids = ids.DistinctBy(x => x);
        foreach (var id in ids)
            tasks.Add(batch.StringSetAsync(GetPriceUpdateKey(id), updateTime.ToString(Global.Culture), _ttl));
        batch.Execute();
        await Task.WhenAll(tasks);
    }

    private static string GetUsablePriceKey(int articleId)
    {
        return $"article-usable-price:{articleId}";
    }

    private static string GetPriceUpdateKey(int articleId)
    {
        return $"article-price-update-date:{articleId}";
    }
}