using System.Linq.Expressions;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticleCoefficients
{
    Task<Dictionary<int, List<ArticleCoefficient>>> GetArticlesCoefficients(IEnumerable<int> articleIds, bool track = true, 
        CancellationToken cancellationToken = default, params Expression<Func<ArticleCoefficient, object>>[] includes);
}