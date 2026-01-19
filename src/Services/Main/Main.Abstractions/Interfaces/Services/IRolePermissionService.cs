namespace Main.Abstractions.Interfaces.Services;

public interface IRolePermissionService
{
    Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetUserPermissionsAsync(Guid userId,
        CancellationToken cancellationToken = default);
}