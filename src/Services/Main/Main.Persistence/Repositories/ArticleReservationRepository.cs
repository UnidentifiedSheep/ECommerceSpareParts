using System.Linq.Expressions;
using Abstractions.Models.Repository;
using Main.Abstractions.Dtos.RepositoryOptionsData;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ArticleReservationRepository(DContext context) : IArticleReservationRepository
{
    public async Task<StorageContentReservation?> GetReservationAsync(
        QueryOptions<StorageContentReservation, int> options,
        CancellationToken cancellationToken = default)
    {
        return await context.StorageContentReservations
            .ApplyOptions(options)
            .FirstOrDefaultAsync(x => x.Id == options.Data, cancellationToken);
    }

    public Task<Dictionary<int, int>> GetReservationsCountForUserAsync(
        Guid userId,
        IEnumerable<int> articleIds,
        CancellationToken cancellationToken = default)
    {
        return GetReservationsCountInternalAsync(
            r => r.UserId == userId && articleIds.Contains(r.ArticleId) && !r.IsDone,
            cancellationToken);
    }

    public Task<Dictionary<int, int>> GetReservationsCountForOthersAsync(
        Guid userId,
        IEnumerable<int> articleIds,
        CancellationToken cancellationToken = default)
    {
        return GetReservationsCountInternalAsync(
            r => r.UserId != userId && articleIds.Contains(r.ArticleId) && !r.IsDone,
            cancellationToken);
    }

    public async Task<Dictionary<int, List<StorageContentReservation>>> GetUserReservations(
        QueryOptions<StorageContentReservation, GetUserReservationsOptionsData> options,
        CancellationToken cancellationToken = default)
    {
        return await context.StorageContentReservations
            .ApplyOptions(options)
            .Where(x => x.UserId == options.Data.UserId &&
                        options.Data.ArticleIds.Contains(x.ArticleId) &&
                        x.IsDone == options.Data.IsDone)
            .GroupBy(x => x.ArticleId)
            .ToDictionaryAsync(g => g.Key,
                g => g.ToList(), cancellationToken);
    }

    private Task<Dictionary<int, int>> GetReservationsCountInternalAsync(
        Expression<Func<StorageContentReservation, bool>> predicate,
        CancellationToken cancellationToken)
    {
        return context.StorageContentReservations
            .AsNoTracking()
            .Where(predicate)
            .GroupBy(x => x.ArticleId)
            .Select(g => new
            {
                ArticleId = g.Key,
                TotalCount = g.Sum(y => y.CurrentCount)
            })
            .ToDictionaryAsync(
                x => x.ArticleId,
                x => x.TotalCount,
                cancellationToken);
    }
}