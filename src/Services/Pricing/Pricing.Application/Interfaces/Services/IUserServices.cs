namespace Pricing.Application.Interfaces.Services;

public interface IUserServices
{
    Task<decimal> GetUserDiscount(
        Guid userId,
        CancellationToken cancellationToken = default);
}