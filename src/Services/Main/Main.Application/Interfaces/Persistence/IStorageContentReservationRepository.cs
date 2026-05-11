using Application.Common.Interfaces.Repositories;
using Main.Entities.Storage;

namespace Main.Application.Interfaces.Persistence;

public interface IStorageContentReservationRepository : IRepository<StorageContentReservation, int>
{
    Task<Dictionary<int, int>> GetReservationsCountForUserAsync(
        Guid userId,
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, int>> GetReservationsCountForOthersAsync(
        Guid userId,
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default);
}