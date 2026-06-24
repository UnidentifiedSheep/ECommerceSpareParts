using Internal.Integration.Core.Models.Main;

namespace Internal.Integration.Core.Interfaces.Main;

public interface ICurrencyNode
{
    Task<decimal> GetCurrencyRate(int currencyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InternalCurrency>> GetCurrencies(CancellationToken cancellationToken = default);
}