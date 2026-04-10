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
            .FirstOrDefaultAsync(x => x.Left == articleId || x.Right == articleId, cancellationToken);
        return pair?.Left == articleId ? pair.ProductRightNavigation : pair?.ProductLeftNavigation;
    }

    public async Task<IEnumerable<ProductPair>> GetRelatedPairsAsync(
        int articleId,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ArticlesPairs.ConfigureTracking(track)
            .Where(x => x.Left == articleId || x.Right == articleId)
            .ToListAsync(cancellationToken);
    }
}