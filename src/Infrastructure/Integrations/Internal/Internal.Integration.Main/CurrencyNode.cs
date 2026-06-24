using System.Text.Json;
using System.Text.Json.Serialization;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Main;

internal sealed class CurrencyNode(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor) 
    : InternalClientBase(authClient, optionsMonitor), ICurrencyNode
{
    public async Task<decimal> GetCurrencyRate(
        int currencyId,
        CancellationToken cancellationToken = default)
    {
        using var request = await GetRequest(
            HttpMethod.Get,
            $"/internal/currencies/{currencyId}/rates",
            cancellationToken);
        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<GetCurrencyRateResponse>(json)?.Rate
               ?? throw new InvalidOperationException($"{nameof(GetCurrencyRate)} returned null.");
    }

    public async Task<IReadOnlyList<InternalCurrency>> GetCurrencies(
        CancellationToken cancellationToken = default)
    {
        using var request = await GetRequest(
            HttpMethod.Get,
            "/currencies",
            cancellationToken);
        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<GetCurrenciesResponse>(json)?.Currencies
               ?? throw new InvalidOperationException($"{nameof(GetCurrencyRate)} returned null.");
    }
    
    private record GetCurrencyRateResponse
    {
        [JsonPropertyName("rate")]
        public decimal Rate { get; init; }
    }

    private record GetCurrenciesResponse
    {
        [JsonPropertyName("currencies")]
        public required IReadOnlyList<InternalCurrency> Currencies { get; init; }
    }
}