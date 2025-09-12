using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface IUsersRepository
{
    Task<IEnumerable<AspNetUser>> GetUsersBySimilarityAsync(double similarityLevel, int page, int viewCount,
        string? name = null, string? surname = null, string? email = null,
        string? phone = null, string? userName = null, string? id = null,
        string? description = null, bool? isSupplier = null, CancellationToken cancellationToken = default);

    Task<decimal?> GetUsersDiscountAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the IDs of users that do not exist in the system.
    /// </summary>
    /// <param name="userIds">A collection of user IDs to check.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of user IDs that are not found.</returns>
    Task<List<string>> UsersExists(IEnumerable<string> userIds, CancellationToken cancellationToken = default);

    Task ChangeUsersDiscount(string userId, decimal discount, CancellationToken cancellationToken = default);
    Task<bool> UserNameTaken(string userName, CancellationToken ct = default);
}