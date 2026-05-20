using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Models.Main;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Main;

public class MainClient(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor)
    : InternalClientBase(authClient, optionsMonitor), IMainClient
{
    public async Task<decimal> GetUserDiscount(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var request = await GetRequest(
            HttpMethod.Get,
            $"/users/{userId}/discount",
            cancellationToken);
        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<GetUserDiscountResponse>(json)?.Discount
               ?? throw new InvalidOperationException($"{nameof(GetUserDiscount)} returned null.");
    }

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

    public async Task<InternalFullProduct?> GetFullProduct(
        int productId,
        CancellationToken cancellationToken = default)
    {
        using var request = await GetRequest(
            HttpMethod.Get,
            $"/internal/products/{productId}/full",
            cancellationToken);
        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<InternalFullProduct>(json);
    }

    private record GetUserDiscountResponse
    {
        [JsonPropertyName("discount")]
        public decimal Discount { get; init; }
    }

    private record GetCurrencyRateResponse
    {
        [JsonPropertyName("rate")]
        public decimal Rate { get; init; }
    }
}
