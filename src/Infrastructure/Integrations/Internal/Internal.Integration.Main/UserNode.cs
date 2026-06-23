using System.Text.Json;
using System.Text.Json.Serialization;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Main;

internal sealed class UserNode(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor) 
    : InternalClientBase(authClient, optionsMonitor), IUserNode
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
    
    private record GetUserDiscountResponse
    {
        [JsonPropertyName("discount")]
        public decimal Discount { get; init; }
    }
}