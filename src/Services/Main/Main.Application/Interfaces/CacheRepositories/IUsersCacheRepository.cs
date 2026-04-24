using Main.Application.Dtos.Users;

namespace Main.Application.Interfaces.CacheRepositories;

public interface IUsersCacheRepository
{
    Task<decimal?> GetUserDiscount(Guid userId);
    Task SetUserDiscount(Guid userId, decimal discount);
    Task<UserDto?> GetUserById(Guid id); 
    Task SetUserById(UserDto user);
    Task<IReadOnlyList<string>> GetUserRoles(Guid userId);
    Task SetUserRoles(Guid userId, IEnumerable<string> roles);
    Task<IReadOnlyList<string>> GetUserPermissions(Guid userId);
    Task SetUserPermissions(Guid userId, IEnumerable<string> permissions);
}