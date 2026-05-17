namespace Internal.Integration.Core.Interfaces;

public interface IMainClient
{
    Task<decimal> GetUserDiscount(Guid userId, CancellationToken cancellationToken = default);
    
    Task<decimal> GetCurrencyRate(int currencyId, CancellationToken cancellationToken = default);
}