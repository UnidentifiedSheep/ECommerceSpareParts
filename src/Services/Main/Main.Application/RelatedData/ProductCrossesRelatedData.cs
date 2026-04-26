using Abstractions.Interfaces.Cache;
using Application.Common.Abstractions.RelatedData;
using Main.Abstractions.Models;
using Main.Entities.Product;

namespace Main.Application.RelatedData;

public class ProductCrossesRelatedData(ICache cache, TimeSpan? ttl = null) : RelatedDataBase<ProductCross>(cache, ttl)
{
    private readonly ICache _cache = cache;
    private readonly TimeSpan? _ttl = ttl;

    public override string GetRelatedDataKey(string id)
    {
        return $"product-crosses-related-data:{id}";
    }

    public override async Task AddRelatedDataAsync(IEnumerable<string> ids, string relatedKey)
    {
        var keys = ids.Select(GetRelatedDataKey).ToHashSet();
        var values = keys.ToList();
        values.Add(relatedKey);
        await _cache.SetAddAsync(keys, values, _ttl);
    }
}