using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Product;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticleImageRepository(DContext context) : IArticleImageRepository
{
    public async Task<IEnumerable<ProductImage>> GetArticlesImages(
        IEnumerable<int> articleIds,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.ProductImages
            .ConfigureTracking(track)
            .Where(x => articleIds.Contains(x.ProductId))
            .ToListAsync(cancellationToken);
    }
}