using System.Linq.Expressions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Storage;
using Main.Enums;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Main.Persistence.Repositories.Storage;

public class StorageContentReservationRepository(DContext context, IQueryableExtensions extensions)
    : LinqRepositoryBase<DContext, StorageContentReservation, int>(context, extensions),
        IStorageContentReservationRepository
{
    public Task<Dictionary<int, int>> GetReservationsCountForUserAsync(
        Guid userId,
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default)
    {
        return GetReservationsCountInternalAsync(
            r =>
                r.UserId == userId &&
                productIds.Contains(r.ProductId) &&
                (r.Status == StorageContentReservationStatus.Active ||
                 r.Status == StorageContentReservationStatus.Locked),
            cancellationToken);
    }

    public Task<Dictionary<int, int>> GetReservationsCountForOthersAsync(
        Guid userId,
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default)
    {
        return GetReservationsCountInternalAsync(
            r =>
                r.UserId != userId &&
                productIds.Contains(r.ProductId) &&
                (r.Status == StorageContentReservationStatus.Active ||
                 r.Status == StorageContentReservationStatus.Locked),
            cancellationToken);
    }

    private Task<Dictionary<int, int>> GetReservationsCountInternalAsync(
        Expression<Func<StorageContentReservation, bool>> predicate,
        CancellationToken cancellationToken)
    {
        return Context.StorageContentReservations
            .AsNoTracking()
            .Where(predicate)
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalCount = g.Sum(y => y.CurrentCount)
            })
            .ToDictionaryAsync(
                x => x.ProductId,
                x => x.TotalCount,
                cancellationToken);
    }
}