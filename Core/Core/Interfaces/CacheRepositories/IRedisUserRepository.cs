namespace Core.Interfaces.CacheRepositories;

public interface IRedisUserRepository
{
    Task<decimal?> GetUserDiscount(Guid userId);
    Task SetUserDiscount(Guid userId, decimal discount);
}