using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticleContentRepository(DContext context) : IArticleContentRepository
{
    public async Task<IEnumerable<ArticlesContent>> GetArticleContents(int articleId, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ArticlesContents.ConfigureTracking(track)
            .Include(x => x.InsideArticle)
            .ThenInclude(x => x.Producer)
            .Where(x => x.MainArticleId == articleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ArticlesContent?> GetArticleContent(int articleId, int insideArticleId, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ArticlesContents.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.MainArticleId == articleId && x.InsideArticleId == insideArticleId,
                cancellationToken);
    }
}