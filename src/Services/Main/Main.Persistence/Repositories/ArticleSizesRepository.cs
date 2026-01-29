using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticleSizesRepository(DContext context) : IArticleSizesRepository
{
    public async Task<ArticleSize?> GetArticleSizes(int articleId, bool track = true, CancellationToken token = default)
    {
        return await context.ArticleSizes.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.ArticleId == articleId, token);
    }

    public async Task<IEnumerable<ArticleSize>> GetArticleSizesByIds(IEnumerable<int> ids, bool track = true, 
        CancellationToken token = default)
    {
        return await context.ArticleSizes.ConfigureTracking(track)
            .Where(x => ids.Contains(x.ArticleId))
            .ToListAsync(token);
    }
}