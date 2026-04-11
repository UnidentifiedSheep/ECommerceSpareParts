using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Product;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticleContentRepository(DContext context) : IArticleContentRepository
{
    public async Task<IEnumerable<ProductContent>> GetArticleContents(
        int articleId,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ProductContents.ConfigureTracking(track)
            .Include(x => x.InsideProduct)
            .ThenInclude(x => x.Producer)
            .Where(x => x.MainArticleId == articleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductContent?> GetArticleContent(
        int articleId,
        int insideArticleId,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ProductContents.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.ParentProductId == articleId && x.ChildProductId == insideArticleId,
                cancellationToken);
    }
}