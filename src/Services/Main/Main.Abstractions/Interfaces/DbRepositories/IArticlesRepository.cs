using Abstractions.Models.Repository;
using Main.Entities;
using Main.Entities.Product;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticlesRepository
{
    Task<Product?> GetArticleById(
        QueryOptions<Product, int> options, 
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetArticleCrosses(
        QueryOptions<Product, int> options,
        CancellationToken cancellationToken = default);

    Task AddArticleLinkage(IEnumerable<(int id, int crossId)> crossIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetArticleCrossIds(int articleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetArticlesByIds(
        QueryOptions<Product, IReadOnlyList<int>> options,
        CancellationToken token = default);

    Task<int> UpdateArticlesCount(Dictionary<int, int> toIncrement, CancellationToken cancellationToken = default);
}