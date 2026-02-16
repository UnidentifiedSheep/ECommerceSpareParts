using Abstractions.Models;

namespace Pricing.Abstractions.Interfaces.CacheRepositories;

public interface IUserCacheRepository
{
    /// <summary>
    /// Sets discount for user
    /// </summary>
    /// <param name="userId">Id of user</param>
    /// <param name="discount">User discount</param>
    /// <param name="setAt">Time when discount set upped. If null than current date will be used.</param>
    Task SetUserDiscount(Guid userId, decimal discount, DateTime? setAt = null);
    /// <summary>
    /// Deletes user discount
    /// </summary>
    /// <param name="userId">Id of user</param>
    Task DeleteUserDiscount(Guid userId);
    /// <summary>
    /// Gets user discount.
    /// </summary>
    /// <param name="userId">Id of user</param>
    /// <returns>If found returns discount, else returns null.</returns>
    Task<Timestamped<decimal>?> GetUserDiscount(Guid userId);
}