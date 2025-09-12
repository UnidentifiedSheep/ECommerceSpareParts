namespace Core.Interfaces.CacheRepositories;

public interface IRedisUserRepository
{
    Task<decimal?> GetUserDiscount(string userId);
    Task SetUserDiscount(string userId, decimal discount);
}