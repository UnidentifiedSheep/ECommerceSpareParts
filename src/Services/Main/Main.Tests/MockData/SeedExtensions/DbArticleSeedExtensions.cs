using Main.Entities;
using Main.Persistence.Context;
using Tests.MockData.DataFactories;

namespace Tests.MockData.SeedExtensions;

public static class DbArticleSeedExtensions
{
    public static async Task<List<Article>> CreateArticles(this DContext ctx, int count, params int[] producerIds)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var articles = ArticleFactory.Create(count, producerIds);
        await ctx.AddRangeAsync(articles);
        await ctx.SaveChangesAsync();
        
        return articles;
    }
}