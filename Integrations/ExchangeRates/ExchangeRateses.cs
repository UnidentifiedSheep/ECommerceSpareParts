using Core.Interfaces.Integrations;
using Core.Models.ExchangeRates;
using Exceptions.Exceptions;
using Integrations.Models.ExchangeRates;
using Mapster;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Integrations.ExchangeRates;

public class ExchangeRates : IExchangeRates
{
    private readonly HttpClient _client;
    private readonly ExchangeRatesOptions _options;
    
    public ExchangeRates(HttpClient client, IOptions<ExchangeRatesOptions> options)
    {
        _client = client;
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.BaseUrl)) 
            throw new ArgumentNullException(nameof(_options.BaseUrl), "Ссылка на сервис обмена валют не может быть пустой");
        
        _client.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<LatestCurrencyRatesResponse> GetRates(IEnumerable<string> currencies, string baseCurrency,
        CancellationToken cancellationToken = default)
    {
        var uri = new UriBuilder(_options.BaseUrl.Trim('/') + "/latest");
        var symbols = string.Join(",", currencies.ToHashSet());
        uri.Query = $"symbols={symbols}&base={baseCurrency}";
        
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri.Uri);
        requestMessage.Headers.Add("apikey", _options.ApiKey);
    
        var response = await _client.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidResponseException("Запрос не был завершен успешно");
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var currencyRates = JsonConvert.DeserializeObject<LatestCurrencyModel>(content)
                            ?? throw new UnableDeserializeException("Unable to deserialize exchange rate data.");
        if (!currencyRates.Success) 
            throw new InvalidResponseException($"Запрос не завершился успехом");
        
        return currencyRates.Adapt<LatestCurrencyRatesResponse>();
    }
}