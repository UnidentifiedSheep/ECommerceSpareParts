using Enums;

namespace Abstractions.Interfaces.Integrations.ExchangeRate;

public interface IExchangeRateClientFactory
{
    IExchangeRateClient GetClient(ExchangeRateProvider provider);
}