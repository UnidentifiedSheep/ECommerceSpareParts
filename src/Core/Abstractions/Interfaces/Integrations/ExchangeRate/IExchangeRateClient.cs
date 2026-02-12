using Abstractions.Models;
using Enums;

namespace Abstractions.Interfaces.Integrations.ExchangeRate;

public interface IExchangeRateClient
{
    ExchangeRateProvider Provider { get; }
    Task<ExchangeRates> GetRates(CancellationToken cancellationToken = default);
}