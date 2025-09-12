using Core.Entities;
using Core.Interfaces.DbRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Extensions;

namespace Persistence.Repositories;

public class ArticleContentRepository(DContext context) : IArticleContentRepository
{
    public async Task<IEnumerable<ArticlesContent>> GetArticleContents(int articleId, bool track = true, CancellationToken cancellationToken = default)
    {
        return await context.ArticlesContents.ConfigureTracking(track)
            .Where(x => x.MainArticleId == articleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ArticlesContent?> GetArticleContentAsync(int articleId, int insideArticleId, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ArticlesContents.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.MainArticleId == articleId && x.InsideArticleId == insideArticleId, cancellationToken);
    }
}