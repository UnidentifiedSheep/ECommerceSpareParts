using Application.Common.Interfaces.Repositories;
using Main.Entities.Storage;

namespace Main.Application.Interfaces.Persistence;

public interface IProductReservationRepository : IRepository<ProductReservation, int>
{
    Task<Dictionary<int, int>> GetReservationsCountForOrganizationAsync(
        Guid organizationId,
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, int>> GetOtherOrganizationsReservationsCountAsync(
        Guid organizationId,
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default);
}
