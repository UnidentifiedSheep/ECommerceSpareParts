using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Product;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticleCharacteristicsRepository(DContext context) : IArticleCharacteristicsRepository
{
    public async Task<IEnumerable<ProductCharacteristic>> GetArticleCharacteristics(
        int articleId,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ProductCharacteristics.ConfigureTracking(track).Where(x => x.ProductId == articleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductCharacteristic>> GetArticleCharacteristicsByIds(
        int? articleId,
        IEnumerable<int> ids,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ProductCharacteristics
            .ConfigureTracking(track)
            .Where(x => ids.Contains(x.ProductId) && (articleId == null || x.ProductId == articleId))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductCharacteristic?> GetCharacteristic(
        int id,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ProductCharacteristics.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.ProductId == id, cancellationToken);
    }
}