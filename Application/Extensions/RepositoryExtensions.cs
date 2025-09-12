using Core.Entities;
using Core.Interfaces.DbRepositories;
using Exceptions.Exceptions.ArticleReservations;
using Exceptions.Exceptions.Articles;
using Exceptions.Exceptions.Balances;
using Exceptions.Exceptions.Currencies;
using Exceptions.Exceptions.Producers;
using Exceptions.Exceptions.Storages;
using Exceptions.Exceptions.Users;

namespace Application.Extensions;

public static class RepositoryExtensions
{
    public static async Task EnsureUsersExists(this IUsersRepository usersRepository, IEnumerable<string> userIds,
        CancellationToken ct = default)
    {
        var notExistingUser = await usersRepository.UsersExists(userIds, ct);
        switch (notExistingUser.Count)
        {
            case 1:
                throw new UserNotFoundException(notExistingUser.First());
            case > 1:
                throw new UserNotFoundException(notExistingUser);
        }
    }

    public static async Task EnsureCurrenciesExists(this ICurrencyRepository repository, IEnumerable<int> currenciesId,
        CancellationToken ct = default)
    {
        var notExistingCurrency = (await repository.CurrenciesExists(currenciesId, ct)).ToList();
        if (notExistingCurrency.Count != 0)
            throw new CurrencyNotFoundException(notExistingCurrency);
    }

    public static async Task EnsureStorageExists(this IStoragesRepository storagesRepository, string storageName,
        CancellationToken ct = default)
    {
        if (!await storagesRepository.StorageExistsAsync(storageName, ct))
            throw new StorageNotFoundException(storageName);
    }

    public static async Task EnsureStoragesExists(this IStoragesRepository storagesRepository,
        IEnumerable<string> storageIds,
        CancellationToken ct = default)
    {
        var notExisting = (await storagesRepository.StoragesExistsAsync(storageIds, ct))
            .ToList();
        switch (notExisting.Count)
        {
            case 1:
                throw new StorageNotFoundException(notExisting.First());
            case > 1:
                throw new StorageNotFoundException(notExisting);
        }
    }

    public static async Task EnsureProducersExists(this IProducerRepository producerRepository,
        IEnumerable<int> producerIds, CancellationToken ct = default)
    {
        var ids = producerIds.Distinct().ToList();
        var notExisting = (await producerRepository.ProducersExistsAsync(ids, ct)).ToList();
        switch (notExisting.Count)
        {
            case 1:
                throw new ProducerNotFoundException(notExisting.First());
            case > 1:
                throw new ProducerNotFoundException(notExisting);
        }
    }

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

    public static async Task EnsureArticlesExist(this IArticlesRepository repository, IEnumerable<int> articleIds,
        CancellationToken cancellationToken = default)
    {
        var notExisting = (await repository.ArticlesExistsAsync(articleIds, cancellationToken)).ToList();
        if (notExisting.Count != 0)
            throw new ArticleNotFoundException(notExisting);
    }

    public static async Task EnsureReservationExists(this IArticleReservationRepository repository, int reservationId,
        CancellationToken ct = default)
    {
        if (!await repository.ReservationExists(reservationId, ct))
            throw new ReservationNotFoundException(reservationId);
    }

    public static async Task EnsureTransactionExists(this IBalanceRepository repository, string transactionId,
        CancellationToken ct = default)
    {
        if (!await repository.TransactionExistsAsync(transactionId, ct))
            throw new TransactionNotFound(transactionId);
    }
}