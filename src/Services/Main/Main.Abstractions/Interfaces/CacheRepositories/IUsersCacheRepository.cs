namespace Main.Abstractions.Interfaces.CacheRepositories;

public interface IUsersCacheRepository
{
    Task<decimal?> GetUserDiscount(Guid userId);
    Task SetUserDiscount(Guid userId, decimal discount);
}