using Abstractions.Models.Repository;
using Main.Abstractions.Exceptions.Articles;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;

namespace Main.Application.Extensions;

public static class RepositoryExtensions
{
    public static async Task<IReadOnlyList<Product>> EnsureArticlesExistsForUpdateAsync(
        this IArticlesRepository articlesRepository,
        IEnumerable<int> articleIds,
        CancellationToken cancellationToken = default)
    {
        var requestedIds = articleIds.ToHashSet();
        var options = new QueryOptions<Product, IReadOnlyList<int>>() { Data = requestedIds.ToList() }
            .WithTracking(false)
            .WithForUpdate();
        var found = await articlesRepository.GetArticlesByIds(options, cancellationToken);

        foreach (var id in found.Select(x => x.Id)) requestedIds.Remove(id);
        return requestedIds.Count != 0 
            ? throw new ArticleNotFoundException(requestedIds) 
            : found;
    }
}