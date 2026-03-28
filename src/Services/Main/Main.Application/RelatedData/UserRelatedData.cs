using Abstractions.Interfaces.Cache;
using Application.Common.Abstractions.RelatedData;
using Main.Entities;

namespace Main.Application.RelatedData;

public class UserRelatedData(ICache cache, TimeSpan? ttl = null) : RelatedDataBase<User>(cache, ttl)
{
    public override string GetRelatedDataKey(string id)
    {
        return $"user-related-data:{id}";
    }
}