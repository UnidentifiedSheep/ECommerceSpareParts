using System.Text.Json.Serialization;
using Integrations.Common;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models;
using Internal.Integration.Core.Models.Main;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Main;

internal sealed class CurrencyNode(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor) 
    : InternalClientBase(authClient, optionsMonitor), ICurrencyNode
{
    public async Task<Response<decimal>> GetCurrencyRate(
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

        return await ReadInternalResponse<GetCurrencyRateResponse, decimal>(
            response,
            x => x.Rate,
            cancellationToken);
    }

    public async Task<Response<IReadOnlyList<InternalCurrency>>> GetCurrencies(
        CancellationToken cancellationToken = default)
    {
        using var request = await GetRequest(
            HttpMethod.Get,
            "/currencies",
            cancellationToken);
        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        return await ReadInternalResponse<GetCurrenciesResponse, IReadOnlyList<InternalCurrency>>(
            response,
            x => x.Currencies,
            cancellationToken);
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
