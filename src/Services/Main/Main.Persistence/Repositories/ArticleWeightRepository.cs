using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Product;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticleWeightRepository(DContext context) : IArticleWeightRepository
{
    public async Task<ProductWeight?> GetArticleWeight(
        int articleId,
        bool track = true,
        CancellationToken token = default)
    {
        return await context.ProductWeights.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.ProductId == articleId, token);
    }

    public async Task<IEnumerable<ProductWeight>> GetArticleWeightsByIds(
        IEnumerable<int> ids,
        bool track = true,
        CancellationToken token = default)
    {
        return await context.ProductWeights.ConfigureTracking(track)
            .Where(x => ids.Contains(x.ProductId))
            .ToListAsync(token);
    }
}