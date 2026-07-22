using System.Linq.Expressions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Storage;
using Main.Enums;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Main.Persistence.Repositories.Storage;

public class ProductReservationRepository(DContext context, IQueryableExtensions extensions)
    : LinqRepositoryBase<DContext, ProductReservation, int>(context, extensions),
        IProductReservationRepository
{
    public Task<Dictionary<int, int>> GetReservationsCountForOrganizationAsync(
        Guid organizationId,
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default)
    {
        return GetReservationsCountInternalAsync(
            r =>
                r.OrganizationId == organizationId &&
                productIds.Contains(r.ProductId) &&
                (r.Status == ProductReservationStatus.Active ||
                 r.Status == ProductReservationStatus.Locked),
            cancellationToken);
    }

    public Task<Dictionary<int, int>> GetOtherOrganizationsReservationsCountAsync(
        Guid organizationId,
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default)
    {
        return GetReservationsCountInternalAsync(
            r =>
                r.OrganizationId != organizationId &&
                productIds.Contains(r.ProductId) &&
                (r.Status == ProductReservationStatus.Active ||
                 r.Status == ProductReservationStatus.Locked),
            cancellationToken);
    }

    private Task<Dictionary<int, int>> GetReservationsCountInternalAsync(
        Expression<Func<ProductReservation, bool>> predicate,
        CancellationToken cancellationToken)
    {
        return Context.ProductReservations
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
