using Core.TransactionBuilder;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.ArticleReservations;
using MonoliteUnicorn.Exceptions.ArticleReservations;
using MonoliteUnicorn.Extensions;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.Services.ArticleReservations;

public class ArticleReservation(DContext context) : IArticleReservation
{
    public async Task CreateReservation(IEnumerable<NewArticleReservationDto> reservations, string whoCreated, CancellationToken cancellationToken = default)
    {
        var reservationsList = reservations.ToList();
        var currencyIds = new HashSet<int>();
        var articleIds = new HashSet<int>();
        var userIds = new HashSet<string> { whoCreated };

        foreach (var item in reservationsList)
        {
            if (item.InitialCount < 0)
                throw new InitialCountMustBeGreaterThenZeroException();
            if (item.InitialCount < item.CurrentCount)
                throw new InitialCountLessOrEqualToCurrentException();
            if (item.CurrentCount <= 0)
                throw new InitialCountLessOrEqualToCurrentException();
            if (item.GivenPrice != null)
            {
                var roundedPrice = Math.Round(item.GivenPrice.Value, 2);
                if (roundedPrice <= 0)
                    throw new GivenPriceMustBePositiveException();
                item.GivenPrice = roundedPrice;
            }
            if(item.GivenCurrencyId != null) 
                currencyIds.Add(item.GivenCurrencyId.Value);
            articleIds.Add(item.ArticleId);
            userIds.Add(item.UserId);
        }
        
        await context.EnsureCurrenciesExist(currencyIds, cancellationToken: cancellationToken);
        await context.EnsureArticlesExist(articleIds, cancellationToken);
        await context.EnsureUserExists(userIds, cancellationToken);
        
        await context.WithDefaultTransactionSettings("normal")
            .ExecuteWithTransaction(async () =>
            {
                var adaptedReservations = reservationsList.Adapt<List<StorageContentReservation>>();
                foreach (var item in adaptedReservations)
                    item.WhoCreated = whoCreated;
                await context.AddRangeAsync(adaptedReservations, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
    }

    public async Task EditReservation(int reservationId, string whoUpdated, EditArticleReservationDto editReservationDto, CancellationToken cancellationToken = default)
    {
        if (editReservationDto.InitialCount <= 0)
            throw new InitialCountMustBeGreaterThenZeroException();
        if (editReservationDto.InitialCount < editReservationDto.CurrentCount)
            throw new InitialCountLessToCurrentException();
        if (editReservationDto.GivenPrice != null && Math.Round(editReservationDto.GivenPrice.Value, 2) <= 0)
            throw new GivenPriceMustBePositiveException();
        
        await context.EnsureArticlesExist(editReservationDto.ArticleId, cancellationToken);
        await context.EnsureReservationExists(reservationId, cancellationToken);
        await context.EnsureUserExists(whoUpdated, cancellationToken);
        if(editReservationDto.GivenCurrencyId != null)
            await context.EnsureCurrencyExists(editReservationDto.GivenCurrencyId.Value, cancellationToken);

        await context.WithDefaultTransactionSettings("normal")
            .ExecuteWithTransaction(async () =>
            {
                var reservation = await context.StorageContentReservations
                    .FirstOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
                
                editReservationDto.Adapt(reservation);
                await context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
    }

    public async Task DeleteReservation(int reservationId, CancellationToken cancellationToken = default)
    {
        await context.EnsureReservationExists(reservationId, cancellationToken);
        await context.Database.ExecuteSqlAsync($"""
                                                   delete
                                                   from storage_content_reservations
                                                   where id = {reservationId};
                                                   """, cancellationToken);
    }

    public async Task<(Dictionary<int, int>, Dictionary<int, int>)> GetArticlesWithNotEnoughStock(string userId, string storageName, bool takeFromOtherStorages, 
        Dictionary<int, int> neededCounts,  CancellationToken cancellationToken = default)
    {
        if (neededCounts.Count == 0) return ([], []);
        var articleIds = neededCounts.Keys.ToList();
        if (neededCounts.Values.Any(x => x <= 0))
            throw new NeededCountCannotBeNegativeException();
        await context.EnsureUserExists(userId, cancellationToken);
        await context.EnsureStorageExists(storageName, cancellationToken);
        await context.EnsureArticlesExist(articleIds, cancellationToken);
        
        //Id артикула и количество которого не хватает, для того чтобы продать не затрагивая чужие резервации
        var resultIncludingReservation = new Dictionary<int, int>();
        var notEnoughStock = new Dictionary<int, int>();
        
        var otherUsersReservations = await context.StorageContentReservations
            .AsNoTracking()
            .Where(x => x.UserId != userId &&
                        articleIds.Contains(x.ArticleId) &&
                        !x.IsDone)
            .GroupBy(x => x.ArticleId)
            .Select(x => new
            {
                ArticleId = x.Key,
                TotalCount = x.Sum(y => y.CurrentCount)
            })
            .ToDictionaryAsync(x => x.ArticleId, 
                x => x.TotalCount, cancellationToken);
        var storageCounts = await context.StorageContents
            .AsNoTracking()
            .Where(x => x.Count > 0 && articleIds.Contains(x.ArticleId) &&
                        (takeFromOtherStorages || x.StorageName == storageName))
            .GroupBy(x => x.ArticleId)
            .Select(g => new
            {
                ArticleId = g.Key,
                TotalCount = g.Sum(x => x.Count)
            })
            .ToDictionaryAsync(x => x.ArticleId, 
                x => x.TotalCount, cancellationToken);
        foreach (var (id, count) in neededCounts)
        {
            storageCounts.TryGetValue(id, out var storageCount);
            otherUsersReservations.TryGetValue(id, out var reservationsCount);
            var stockDiff = storageCount - count;
            if (stockDiff < 0)
            {
                notEnoughStock.Add(id, Math.Abs(stockDiff));
                continue;
            }
            var reservationsDiff = stockDiff - reservationsCount;
            if(reservationsDiff < 0) resultIncludingReservation.Add(id, Math.Abs(reservationsDiff));
        }
        return (resultIncludingReservation, notEnoughStock);
    }

    public async Task SubtractCountFromReservations(string userId, string whoUpdated, Dictionary<int, int> contents, 
        CancellationToken cancellationToken = default)
    {
        if (contents.Count == 0) return;
        await context.EnsureUserExists([userId, whoUpdated], cancellationToken);
        var articleIds = contents.Keys.ToHashSet();
        await context.WithDefaultTransactionSettings("normal")
            .ExecuteWithTransaction(async () =>
            {
                foreach (var articleId in articleIds)
                {
                    if (contents[articleId] <= 0) continue;
                    
                    await foreach (var reservation in context.StorageContentReservations
                                       .FromSql($"""
                                                 Select * from storage_content_reservations where user_id = {userId} and
                                                 article_id = {articleId} and is_done = {false}
                                                 order by create_at asc 
                                                 for update
                                                 """).AsAsyncEnumerable().WithCancellation(cancellationToken))
                    {
                        var subtractCount = contents[articleId];
                        if (subtractCount <= 0)
                            break;

                        var min = Math.Min(subtractCount, reservation.CurrentCount);
                        reservation.CurrentCount -= min;
                        contents[articleId] -= min;

                        if (reservation.CurrentCount == 0)
                            reservation.IsDone = true;
                        reservation.WhoUpdated = whoUpdated;
                        reservation.UpdatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                    }
                }
                await context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
    }
}