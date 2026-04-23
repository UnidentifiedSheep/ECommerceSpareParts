using System.Linq.Expressions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Storage;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Main.Persistence.Repositories.Storage;

public class StorageContentReservationRepository(DContext context) 
    : RepositoryBase<DContext, StorageContentReservation, int>(context), IStorageContentReservationRepository
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
                !r.IsDone,
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
                !r.IsDone,
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