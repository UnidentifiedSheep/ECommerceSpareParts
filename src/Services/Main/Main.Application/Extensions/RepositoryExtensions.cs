using Exceptions.Exceptions.Articles;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;

namespace Main.Application.Extensions;

public static class RepositoryExtensions
{
    public static async Task<Dictionary<int, Article>> EnsureArticlesExistForUpdate(this IArticlesRepository repository,
        IEnumerable<int> articleIds,
        bool track = true, CancellationToken cancellationToken = default)
    {
        var ids = articleIds.ToList();
        var articles = (await repository.GetArticlesForUpdate(ids, track, cancellationToken))
            .ToDictionary(x => x.Id);
        var notFoundArticles = ids.Except(articles.Select(x => x.Key)).ToList();
        if (notFoundArticles.Count != 0)
            throw new ArticleNotFoundException(notFoundArticles);
        return articles;
    }
}