using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Services;

namespace Main.Application.Services;

public class RolePermissionService(IUserRoleRepository userRoleRepository, IUserPermissionRepository userPermissionRepository) 
    : IRolePermissionService
{
    public async Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetUserPermissionsAsync(Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var userRoles = await userRoleRepository.GetUserRolesAsync(userId, false, 
            null, null, cancellationToken);
        var permissions = new List<string>();
        var roles = new List<string>();
        
        foreach (var userRole in userRoles)
        {
            permissions.AddRange(userRole.Role.PermissionNames.Select(x => x.Name));
            roles.Add(userRole.Role.NormalizedName);
        }

        var privatePermissions =
            (await userPermissionRepository.GetUserPermissionsAsync(userId, false, cancellationToken))
            .Select(x => x.Name);
        
        permissions.AddRange(privatePermissions);
        
        return (roles, permissions);
    }
}