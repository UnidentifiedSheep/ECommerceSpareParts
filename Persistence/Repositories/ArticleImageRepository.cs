using Core.Entities;
using Core.Interfaces.DbRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Extensions;

namespace Persistence.Repositories;

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