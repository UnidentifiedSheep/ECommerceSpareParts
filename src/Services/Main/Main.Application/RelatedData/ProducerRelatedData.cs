using Core.Abstractions;
using Core.Interfaces.CacheRepositories;
using Main.Entities;

namespace Main.Application.RelatedData;

public class ProducerRelatedData(ICache cache, TimeSpan? ttl = null) : RelatedDataBase<Producer>(cache, ttl)
{
    public override string GetRelatedDataKey(string id)
    {
        return $"producer-related-data:{id}";
    }
}