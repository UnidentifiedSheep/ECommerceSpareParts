using Enums;
using Integrations.ExchangeRate.Interfaces;

namespace Integrations.ExchangeRate;

public class ExchangeRateClientFactory : IExchangeRateClientFactory
{
	private readonly Dictionary<ExchangeRateProvider, IExchangeRateClient> _clients;

	public ExchangeRateClientFactory(IEnumerable<IExchangeRateClient> clients)
	{
		_clients = clients.ToDictionary(c => c.Provider);
	}

	public IExchangeRateClient GetClient(ExchangeRateProvider provider)
		=> _clients.TryGetValue(provider, out var client)
			? client
			: throw new InvalidOperationException("Unable to find client for provider: " + provider);
}
