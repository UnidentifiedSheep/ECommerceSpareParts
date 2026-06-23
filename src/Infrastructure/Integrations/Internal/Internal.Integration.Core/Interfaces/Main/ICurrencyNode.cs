namespace Internal.Integration.Core.Interfaces.Main;

public interface ICurrencyNode
{
    Task<decimal> GetCurrencyRate(int currencyId, CancellationToken cancellationToken = default);
}