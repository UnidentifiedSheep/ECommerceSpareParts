using Core.Abstractions;
using Core.Interfaces.CacheRepositories;
using Main.Core.Entities;

namespace Main.Application.RelatedData;

public class CurrencyRelatedData(ICache cache, TimeSpan? ttl = null) : RelatedDataBase<Currency>(cache, ttl)
{
    public override string GetRelatedDataKey(string id)
    {
        return $"currency-related-data:{id}";
    }
}