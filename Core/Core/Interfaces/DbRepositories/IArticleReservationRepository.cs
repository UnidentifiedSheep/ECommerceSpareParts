using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface IArticleReservationRepository
{
    Task<bool> ReservationExists(int reservationId, CancellationToken cancellationToken = default);
    Task<StorageContentReservation?> GetReservationAsync(int reservationId, bool track = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<StorageContentReservation>> GetReservationsByExecAsync(string? searchTerm, string? userId, 
        int offset, int limit, string? sortBy, bool track = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<StorageContentReservation>> GetReservationsBySimilarityAsync(string? searchTerm, string? userId, 
        int offset, int limit, string? sortBy, bool track = true, double similarity = 0.5, CancellationToken cancellationToken = default);
    Task<IEnumerable<StorageContentReservation>> GetReservationsFromStartAsync(string? searchTerm, string? userId, 
        int offset, int limit, string? sortBy, bool track = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<StorageContentReservation>> GetReservationsContainsAsync(string? searchTerm, string? userId, 
        int offset, int limit, string? sortBy, bool track = true, CancellationToken cancellationToken = default);
    Task<Dictionary<int, int>> GetReservationsCountForUserAsync(string userId, IEnumerable<int> articleIds, CancellationToken cancellationToken = default);

    Task<Dictionary<int, int>> GetReservationsCountForOthersAsync(string userId, IEnumerable<int> articleIds, CancellationToken cancellationToken = default);
    Task<Dictionary<int, List<StorageContentReservation>>> GetUserReservations(
        string userId, IEnumerable<int> articleIds, bool isDone = false, bool track = true, CancellationToken cancellationToken = default);
    Task<Dictionary<int, List<StorageContentReservation>>> GetUserReservationsForUpdate(
        string userId, IEnumerable<int> articleIds, bool isDone = false, bool track = true, CancellationToken cancellationToken = default);
}