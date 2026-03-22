using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Tests.MockData.DataFactories;
using Tests.MockData.SeedExtensions;

namespace Tests.MockData.ScenatioExtensions;

public static class ArticleScenarioExtensions
{
    public static async Task<(List<Producer>, List<Article>)> CreateProducerAndArticles(this DContext ctx, 
        int producersCount, int articlesCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(producersCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(articlesCount);
        
        var producers = await ctx.CreateProducers(producersCount);
        var producerIds = producers.Select(x => x.Id).ToArray();

        var articles = await ctx.CreateArticles(articlesCount, producerIds);
        return (producers, articles);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="count">count of storage contents per article</param>
    /// <param name="currencyIds"></param>
    /// <param name="articleIds"></param>
    /// <param name="storageNames"></param>
    public static async Task AddStorageContentsAndIncreaseArticleCounts(this DContext ctx, int count, IEnumerable<int> currencyIds, 
        IEnumerable<int> articleIds, IEnumerable<string> storageNames)
    {
        var currencyList = currencyIds.ToList();
        var articleList = articleIds.ToList();
        var storageList = storageNames.ToList();
        
        var articles = await ctx.Articles
            .Where(x => articleList.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);
        foreach (var (id, article) in articles)
        {
            var storageContents = StorageContentFactory
                .Create(count, currencyList, [id], storageList);
            
            article.TotalCount += storageContents.Sum(x => x.Count);
            
            await ctx.AddRangeAsync(storageContents);
        }
        
        await ctx.SaveChangesAsync();
    }
}