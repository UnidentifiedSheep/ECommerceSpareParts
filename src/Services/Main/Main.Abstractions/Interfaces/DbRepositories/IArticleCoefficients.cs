using System.Linq.Expressions;
using Main.Entities;
using Main.Entities.Product;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticleCoefficients
{
    Task<Dictionary<int, List<ProductCoefficient>>> GetArticlesCoefficients(
        IEnumerable<int> articleIds,
        bool track = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<ProductCoefficient, object>>[] includes);
}