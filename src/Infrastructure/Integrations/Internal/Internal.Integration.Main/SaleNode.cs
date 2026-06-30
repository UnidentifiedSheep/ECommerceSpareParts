using Integrations.Common;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models;
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
    public async Task<Response<InternalFullSale>> GetFullSale(
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

        return await ReadInternalResponse<InternalFullSale>(response, cancellationToken);
    }
}
