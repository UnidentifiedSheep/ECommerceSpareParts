using System.Text.Json;
using Abstractions.Interfaces.Integrations.ExchangeRate;
using Abstractions.Models;
using Enums;
using ExchangeRate.Models;

namespace ExchangeRate.Clients;

public class CbrClient(IHttpClientFactory clientFactory) : IExchangeRateClient
{
    private readonly HttpClient _client = clientFactory.CreateClient(nameof(ExchangeRateProvider.Cbr));

    public ExchangeRateProvider Provider => ExchangeRateProvider.Cbr;

    public async Task<ExchangeRates> GetRates(CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync("", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new Exception("Ошибка при получении курсов валют от ЦБР");
        
        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        CbrRatesResponse? result = JsonSerializer.Deserialize<CbrRatesResponse>(json);
        
        if (result == null) throw new Exception("Сервер вернул пустой ответ.");
        return new ExchangeRates(result.Base, result.Rates);
    }
}