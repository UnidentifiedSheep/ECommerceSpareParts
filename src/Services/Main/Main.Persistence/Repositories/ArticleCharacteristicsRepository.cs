using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticleCharacteristicsRepository(DContext context) : IArticleCharacteristicsRepository
{
    public async Task<IEnumerable<ArticleCharacteristic>> GetArticleCharacteristics(int articleId, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ArticleCharacteristics.ConfigureTracking(track).Where(x => x.ArticleId == articleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ArticleCharacteristic?> GetCharacteristic(int id, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ArticleCharacteristics.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}