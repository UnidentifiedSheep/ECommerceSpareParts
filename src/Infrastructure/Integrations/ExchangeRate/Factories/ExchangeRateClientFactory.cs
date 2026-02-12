using Abstractions.Interfaces.Integrations.ExchangeRate;
using Enums;

namespace ExchangeRate.Factories;

public class ExchangeRateClientFactory : IExchangeRateClientFactory
{
    private readonly Dictionary<ExchangeRateProvider, IExchangeRateClient> _clients;

    public ExchangeRateClientFactory(IEnumerable<IExchangeRateClient> clients)
    {
        _clients = clients.ToDictionary(c => c.Provider);
    }

    public IExchangeRateClient GetClient(ExchangeRateProvider provider)
    {
        if (_clients.TryGetValue(provider, out var client)) return client;
        throw new Exception($"Не удалось найти клиент для указанного провайдера: {provider}");
    }
}