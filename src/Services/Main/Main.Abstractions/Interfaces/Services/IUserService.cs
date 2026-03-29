using Main.Abstractions.Dtos.Users;

namespace Main.Abstractions.Interfaces.Services;

public interface IUserService
{
    /// <summary>
    /// Trys to get user from cache, if not found gets from db.
    /// When user taken from db, than it's not gonna be tracked via ef core.
    /// </summary>
    /// <param name="userId">Id of user to find</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>User if found, null if not.</returns>
    Task<FullUserDto?> TryGetUserAsync(Guid userId, CancellationToken token = default);

    Task<decimal?> GetUserDiscountAsync(Guid userId, CancellationToken token = default);

    Task<UserRolesAndPermissions> GetUserRolesAndPermissionsAsync(
        Guid userId,
        CancellationToken token = default);
}