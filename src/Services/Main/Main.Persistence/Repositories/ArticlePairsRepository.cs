using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticlePairsRepository(DContext context) : IArticlePairsRepository
{
    public async Task<Product?> GetArticlePairAsync(
        int articleId,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        var pair = await context.ArticlesPairs
            .ConfigureTracking(track)
            .Include(articlesPair => articlesPair.ProductRightNavigation)
            .Include(articlesPair => articlesPair.ProductLeftNavigation)
            .FirstOrDefaultAsync(x => x.ArticleLeft == articleId || x.ArticleRight == articleId, cancellationToken);
        return pair?.ArticleLeft == articleId ? pair.ProductRightNavigation : pair?.ProductLeftNavigation;
    }

    public async Task<IEnumerable<ArticlesPair>> GetRelatedPairsAsync(
        int articleId,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ArticlesPairs.ConfigureTracking(track)
            .Where(x => x.ArticleLeft == articleId || x.ArticleRight == articleId)
            .ToListAsync(cancellationToken);
    }
}