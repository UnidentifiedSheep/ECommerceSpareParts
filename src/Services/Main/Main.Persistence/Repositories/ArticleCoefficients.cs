using System.Linq.Expressions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticleCoefficients(DContext context) : IArticleCoefficients
{
    public async Task<Dictionary<int, List<ArticleCoefficient>>> GetArticlesCoefficients(IEnumerable<int> articleIds, 
        bool track = true, CancellationToken cancellationToken = default, params Expression<Func<ArticleCoefficient, object>>[] includes)
    {
        var query = context.ArticleCoefficients
            .ConfigureTracking(track)
            .Where(x => articleIds.Contains(x.ArticleId));
        
        foreach (var include in includes)
            query = query.Include(include);

        return await query.GroupBy(x => x.ArticleId)
            .ToDictionaryAsync(x => x.Key,
                x => x.ToList(), cancellationToken);
    }
}