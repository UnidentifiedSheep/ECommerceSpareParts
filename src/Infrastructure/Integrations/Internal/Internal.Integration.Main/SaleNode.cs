using System.Net;
using System.Text.Json;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main;
using Internal.Integration.Core.Models.Main.Sale;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Main;

internal sealed class SaleNode(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor)
    : InternalClientBase(authClient, optionsMonitor), ISaleNode
{
    public async Task<InternalFullSale?> GetFullSale(
        Guid saleId,
        CancellationToken cancellationToken = default)
    {
        using var request = await GetRequest(
            HttpMethod.Get,
            $"/internal/sales/{saleId}",
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
        return JsonSerializer.Deserialize<InternalFullSale>(json);
    }
}
