using System.Linq.Expressions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Product;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticleCoefficients(DContext context) : IArticleCoefficients
{
    public async Task<Dictionary<int, List<ProductCoefficient>>> GetArticlesCoefficients(
        IEnumerable<int> articleIds,
        bool track = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<ProductCoefficient, object>>[] includes)
    {
        var query = context.ProductCoefficients
            .ConfigureTracking(track)
            .Where(x => articleIds.Contains(x.ProductId));

        foreach (var include in includes)
            query = query.Include(include);

        return await query.GroupBy(x => x.ProductId)
            .ToDictionaryAsync(x => x.Key,
                x => x.ToList(), cancellationToken);
    }
}