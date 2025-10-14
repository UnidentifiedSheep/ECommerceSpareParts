using Exceptions.Exceptions.ArticleReservations;
using Exceptions.Exceptions.Articles;
using Exceptions.Exceptions.Balances;
using Exceptions.Exceptions.Currencies;
using Exceptions.Exceptions.Producers;
using Exceptions.Exceptions.Storages;
using Exceptions.Exceptions.Users;
using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;

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