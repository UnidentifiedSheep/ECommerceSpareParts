using Abstractions.Models.Repository;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticlesRepository
{
    Task<Article?> GetArticleById(
        QueryOptions<Article, int> options, 
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Article>> GetArticleCrosses(
        QueryOptions<Article, int> options,
        CancellationToken cancellationToken = default);

    Task AddArticleLinkage(IEnumerable<(int id, int crossId)> crossIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetArticleCrossIds(int articleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Article>> GetArticlesByIds(
        QueryOptions<Article, IReadOnlyList<int>> options,
        CancellationToken token = default);

    Task<int> UpdateArticlesCount(Dictionary<int, int> toIncrement, CancellationToken cancellationToken = default);
}