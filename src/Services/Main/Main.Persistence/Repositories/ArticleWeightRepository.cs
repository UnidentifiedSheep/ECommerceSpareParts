using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticleWeightRepository(DContext context) : IArticleWeightRepository
{
    public async Task<ArticleWeight?> GetArticleWeight(int articleId, bool track = true, CancellationToken token = default)
    {
        return await context.ArticleWeights.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.ArticleId == articleId, token);
    }

    public async Task<IEnumerable<ArticleWeight>> GetArticleWeightsByIds(IEnumerable<int> ids, bool track = true, 
        CancellationToken token = default)
    {
        return await context.ArticleWeights.ConfigureTracking(track)
            .Where(x => ids.Contains(x.ArticleId))
            .ToListAsync(token);
    }
}