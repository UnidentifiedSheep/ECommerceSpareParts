using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Exceptions.ArticleReservations;
using MonoliteUnicorn.Exceptions.Articles;
using MonoliteUnicorn.Exceptions.Currencies;
using MonoliteUnicorn.Exceptions.Producers;
using MonoliteUnicorn.Exceptions.Storages;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.Extensions;

public static class EntityExistenceValidator
{
    public static async Task EnsureProducerExists(this DContext context, int producerId, CancellationToken cancellationToken = default)
    {
        var exists = await context.Producers.AsNoTracking()
            .AnyAsync(x => x.Id == producerId, cancellationToken);
        if (!exists) throw new ProducerNotFoundException(producerId);
    }

    public static async Task EnsureUserExists(this DContext context, string userId, CancellationToken cancellationToken = default)
    {
        var userExists = await context.AspNetUsers.AsNoTracking()
            .AnyAsync(x => x.Id == userId, cancellationToken);
        if (!userExists) throw new UserNotFoundException(userId);
    }
    
    public static async Task EnsureUserExists(this DContext context, IEnumerable<string> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds.ToHashSet();
        var users = await context.AspNetUsers.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        var missedUsers= ids.Except(users).ToList();
        if (missedUsers.Count != 0) throw new UserNotFoundException(missedUsers);
    }

    public static async Task EnsureStorageExists(this DContext context, string storageName,
        CancellationToken cancellationToken = default)
    {
        var storageExists =await context.Storages.AsNoTracking()
                .AnyAsync(x => x.Name == storageName, cancellationToken);
        if (!storageExists) throw new StorageNotFoundException(storageName);
    }

    public static async Task EnsureStoragesExist(this DContext context, IEnumerable<string> storageNames,
        CancellationToken cancellationToken = default)
    {
        var hs = storageNames.ToHashSet();
        var storages = await context.Storages
            .AsNoTracking()
            .Where(x => hs.Contains(x.Name))
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);
        var missedStorages = hs.Except(storages).ToList();
        if (missedStorages.Count > 0) throw new StorageNotFoundException(missedStorages);
    }

    public static async Task<Dictionary<int, StorageContent>> EnsureStorageContentsExistForUpdate(this DContext context, IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        var hs = ids.ToHashSet();
        var storageContents = await context.StorageContents
            .FromSql($"Select * from storage_content where id = Any({hs.ToArray()}) for update")
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var missingIds = hs.Except(storageContents.Keys).ToList();
        if (missingIds.Count != 0) throw new StorageContentNotFoundException(missingIds);
        return storageContents;
    }

    public static async Task EnsureCurrencyExists(this DContext context, int currencyId,
        CancellationToken cancellationToken = default)
    {
        var currencyExists = await context.Currencies.AsNoTracking()
            .AnyAsync(x => x.Id == currencyId, cancellationToken);
        if (!currencyExists)
            throw new CurrencyNotFoundException(currencyId);
    }
    
    public static async Task EnsureCurrenciesExist(this DContext context, IEnumerable<int> currencyIds,
        CancellationToken cancellationToken = default)
    {
        var hs = currencyIds.ToHashSet();
        var currencies = await context.Currencies
            .AsNoTracking()
            .Where(x => hs.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
            
        var missedCurrencies = hs.Except(currencies).ToList();
        if (missedCurrencies.Count != 0) throw new CurrencyNotFoundException(missedCurrencies);
    }

    public static async Task<Dictionary<int, Article>> EnsureArticlesExistForUpdate(this DContext context, IEnumerable<int> articleIds, 
        CancellationToken cancellationToken = default)
    {
        var ids = articleIds.ToHashSet();
        var articles = await context.Articles
            .FromSql($"SELECT * from articles where id = ANY ({ids.ToArray()}) for update;")
            .ToDictionaryAsync(x => x.Id,cancellationToken);
        var notFoundArticles = ids.Except(articles.Select(x => x.Key)).ToList();
        if (notFoundArticles.Count != 0)
            throw new ArticleNotFoundException(notFoundArticles);
        return articles;
    }
    
    public static async Task EnsureArticlesExist(this DContext context, IEnumerable<int> articleIds, 
        CancellationToken cancellationToken = default)
    {
        var ids = articleIds.ToHashSet();
        var articles = await context.Articles
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken: cancellationToken);
        var notFoundArticles = ids.Except(articles).ToList();
        if (notFoundArticles.Count != 0)
            throw new ArticleNotFoundException(notFoundArticles);
    }
    
    public static async Task EnsureArticlesExist(this DContext context, int articleId, 
        CancellationToken cancellationToken = default)
    {
        var exists = await context.Articles
            .AnyAsync(x => x.Id == articleId, cancellationToken);
        if (!exists)
            throw new ArticleNotFoundException(articleId);
    }

    public static async Task<int> UpdateArticlesCount(this DContext context, Dictionary<int, int> toIncrement, CancellationToken cancellationToken = default)
    {
        var valuesSql = string.Join(", ", toIncrement.Select(i => $"({i.Key}, {i.Value})"));
            
        var query = $"""
                     UPDATE articles a
                     SET total_count = a.total_count + data.increment
                     FROM (VALUES {valuesSql}) AS data(article_id, increment)
                     WHERE a.id = data.article_id
                     """;
        return await context.Database.ExecuteSqlRawAsync(query, cancellationToken);
    }

    public static async Task EnsureReservationExists(this DContext context, int reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservationExists = await context.StorageContentReservations
            .AsNoTracking()
            .AnyAsync(x => x.Id == reservationId, cancellationToken);
        if (!reservationExists)
            throw new ReservationNotExistsException(reservationId);
    }
}