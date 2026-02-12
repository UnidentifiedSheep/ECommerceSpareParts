using Abstractions.Interfaces.Cache;
using Application.Common.Abstractions.RelatedData;
using Main.Entities;

namespace Main.Application.RelatedData;

public class CurrencyRelatedData(ICache cache, TimeSpan? ttl = null) : RelatedDataBase<Currency>(cache, ttl)
{
    public override string GetRelatedDataKey(string id)
    {
        return $"currency-related-data:{id}";
    }
}