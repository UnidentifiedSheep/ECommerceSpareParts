using Enums;
using Integrations.Common;
using Integrations.ExchangeRate.Models;

namespace Integrations.ExchangeRate.Interfaces;

public interface IExchangeRateClient
{
	ExchangeRateProvider Provider { get; }

	Task<Response<ExchangeRates>> GetRates(CancellationToken cancellationToken = default);
}
