using Internal.Integration.Core.Models;
using Internal.Integration.Core.Models.Main;

namespace Internal.Integration.Core.Interfaces.Main;

public interface ICurrencyNode
{
    Task<InternalResponse<decimal>> GetCurrencyRate(int currencyId, CancellationToken cancellationToken = default);
    Task<InternalResponse<IReadOnlyList<InternalCurrency>>> GetCurrencies(CancellationToken cancellationToken = default);
}