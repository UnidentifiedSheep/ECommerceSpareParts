using Abstractions.Models.Repository;
using Main.Abstractions.Dtos.RepositoryOptionsData;
using Main.Entities;
using Main.Entities.Storage;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticleReservationRepository
{
    Task<StorageContentReservation?> GetReservationAsync(
        QueryOptions<StorageContentReservation, int> options,
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, int>> GetReservationsCountForUserAsync(
        Guid userId,
        IEnumerable<int> articleIds,
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, int>> GetReservationsCountForOthersAsync(
        Guid userId,
        IEnumerable<int> articleIds,
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, List<StorageContentReservation>>> GetUserReservations(
        QueryOptions<StorageContentReservation, GetUserReservationsOptionsData> options,
        CancellationToken cancellationToken = default);
}