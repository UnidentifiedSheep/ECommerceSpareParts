using Main.Entities;

namespace Main.Abstractions.Interfaces.CacheRepositories;

public interface IUsersCacheRepository
{
    Task<decimal?> GetUserDiscount(Guid userId);
    Task SetUserDiscount(Guid userId, decimal discount);
    Task<User?> GetUserByEmail(string email); 
    Task SetUserByEmail(string email, User user);
}