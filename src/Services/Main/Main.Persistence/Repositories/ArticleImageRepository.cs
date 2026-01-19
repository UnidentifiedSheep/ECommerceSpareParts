using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticleImageRepository(DContext context) : IArticleImageRepository
{
    public async Task<IEnumerable<ArticleImage>> GetArticlesImages(IEnumerable<int> articleIds, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ArticleImages
            .ConfigureTracking(track)
            .Where(x => articleIds.Contains(x.ArticleId))
            .ToListAsync(cancellationToken);
    }
}