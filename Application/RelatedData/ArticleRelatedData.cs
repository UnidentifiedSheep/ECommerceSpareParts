using Core.Abstractions;
using Core.Entities;
using Core.Interfaces.CacheRepositories;

namespace Application.RelatedData;

public class ArticleRelatedData(ICache cache, TimeSpan? ttl = null) : RelatedDataBase<Article>(cache, ttl)
{
    public override string GetRelatedDataKey(string id)
    {
        return $"article-related-data:{id}";
    }
}