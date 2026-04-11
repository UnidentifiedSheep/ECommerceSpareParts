using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Product;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticleSizesRepository(DContext context) : IArticleSizesRepository
{
    public async Task<ProductSize?> GetArticleSizes(int articleId, bool track = true, CancellationToken token = default)
    {
        return await context.ProductSizes.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.ProductId == articleId, token);
    }

    public async Task<IEnumerable<ProductSize>> GetArticleSizesByIds(
        IEnumerable<int> ids,
        bool track = true,
        CancellationToken token = default)
    {
        return await context.ProductSizes.ConfigureTracking(track)
            .Where(x => ids.Contains(x.ProductId))
            .ToListAsync(token);
    }
}