using System.Net;
using System.Text.Json;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Models.Main;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Main;

internal sealed class PurchaseNode(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor) : InternalClientBase(authClient, optionsMonitor)
{
    public async Task<InternalFullPurchase?> GetFullPurchase(
        Guid purchaseId,
        CancellationToken cancellationToken = default)
    {
        using var request = await GetRequest(
            HttpMethod.Get,
            $"/internal/purchases/{purchaseId}",
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
        return JsonSerializer.Deserialize<InternalFullPurchase>(json);
    }
}
