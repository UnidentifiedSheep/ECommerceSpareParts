using MonoliteUnicorn.PostGres.Main;

namespace Tests.MockData;

public static class MockExtensions
{
    public static async Task AddArticleCross(this DContext context, int id1, int id2)
    {
        context.ArticleCrosses.AddRange(
            new ArticleCross { ArticleId = id1, ArticleCrossId = id2 },
            new ArticleCross { ArticleId = id2, ArticleCrossId = id1 }
        );
        await context.SaveChangesAsync();
    }
}