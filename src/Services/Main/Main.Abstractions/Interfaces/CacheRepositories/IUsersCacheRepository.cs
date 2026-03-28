using Main.Entities;

namespace Main.Abstractions.Interfaces.CacheRepositories;

public interface IUsersCacheRepository
{
    Task<decimal?> GetUserDiscount(Guid userId);
    Task SetUserDiscount(Guid userId, decimal discount);
    Task<User?> GetUserById(Guid id); 
    Task SetUserById(User user);
    Task<IReadOnlyList<string>> GetUserRoles(Guid userId);
    Task SetUserRoles(Guid userId, IEnumerable<string> roles);
    Task<IReadOnlyList<string>> GetUserPermissions(Guid userId);
    Task SetUserPermissions(Guid userId, IEnumerable<string> permissions);
}