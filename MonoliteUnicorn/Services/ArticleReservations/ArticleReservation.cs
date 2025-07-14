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
            if (item.InitialCount <= item.CurrentCount)
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

    public async Task IsCountEnoughForUser(int articleId, string userId, int neededCount,  CancellationToken cancellationToken = default)
    {
        
    }
}