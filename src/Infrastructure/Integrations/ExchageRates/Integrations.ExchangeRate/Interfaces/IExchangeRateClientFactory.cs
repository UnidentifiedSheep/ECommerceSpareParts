using Enums;

namespace Integrations.ExchangeRate.Interfaces;

public interface IExchangeRateClientFactory
{
	IExchangeRateClient GetClient(ExchangeRateProvider provider);
}
