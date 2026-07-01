using Integrations.Common;
using Internal.Integration.Core.Models.Main;

namespace Internal.Integration.Core.Interfaces.Main;

public interface ICurrencyNode
{
    Task<Response<decimal>> GetCurrencyRate(int currencyId, CancellationToken cancellationToken = default);

    Task<Response<IReadOnlyList<InternalCurrency>>> GetCurrencies(
        CancellationToken cancellationToken = default);
}