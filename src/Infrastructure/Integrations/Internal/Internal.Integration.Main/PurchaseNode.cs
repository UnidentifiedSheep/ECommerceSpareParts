using Integrations.Common;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main.Purchase;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Main;

internal sealed class PurchaseNode(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor
)
    : InternalClientBase(authClient, optionsMonitor), IPurchaseNode
{
    public async Task<Response<InternalFullPurchase>> GetFullPurchase(
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

        return await ReadResponse<InternalFullPurchase>(response, cancellationToken);
    }
}