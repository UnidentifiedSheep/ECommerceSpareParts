using Core.Entities;
using Core.Interfaces.DbRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Extensions;

namespace Persistence.Repositories;

public class ArticlePairsRepository(DContext context) : IArticlePairsRepository
{
    public async Task<Article?> GetArticlePairAsync(int articleId, bool track = true, CancellationToken cancellationToken = default)
    {
        var pair = await context.ArticlesPairs
            .ConfigureTracking(track)
            .Include(articlesPair => articlesPair.ArticleRightNavigation)
            .Include(articlesPair => articlesPair.ArticleLeftNavigation)
            .FirstOrDefaultAsync(x => x.ArticleLeft == articleId || x.ArticleRight == articleId, cancellationToken);
        return pair?.ArticleRightNavigation ?? pair?.ArticleLeftNavigation;
    }

    public async Task<IEnumerable<ArticlesPair>> GetRelatedPairsAsync(int articleId, bool track = true, CancellationToken cancellationToken = default) 
        => await context.ArticlesPairs.ConfigureTracking(track)
            .Where(x => x.ArticleLeft == articleId || x.ArticleRight == articleId)
            .ToListAsync(cancellationToken);
}