using Main.Application.Dtos.Users;

namespace Main.Application.Interfaces.Cache;

public interface IUserCacheRepository
{
    Task<UserDto?> TryGetUserAsync(
        Guid userId,
        CancellationToken token = default);

    Task<decimal?> GetUserDiscountAsync(
        Guid userId,
        CancellationToken token = default);

    Task<UserRolesAndPermissions?> GetUserRolesAndPermissionsAsync(
        Guid userId,
        CancellationToken token = default);

    Task InvalidateUserAsync(Guid userId);

    Task InvalidateUsersAsync(IEnumerable<Guid> userIds);

    Task InvalidateUserDiscountAsync(Guid userId);

    Task InvalidateUserRolesAndPermissionsAsync(Guid userId);

    Task InvalidateRolesAsync();
}