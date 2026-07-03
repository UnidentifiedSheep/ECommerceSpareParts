using System.Text.Json.Serialization;
using Integrations.Common;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main.Product;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Main;

internal sealed class ProductNode(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor
) : InternalClientBase(authClient, optionsMonitor), IProductNode
{
    public async Task<Response<IReadOnlyList<InternalFullProduct>>> GetFullProduct(
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default)
    {
        var ids = productIds.Select(x => x.ToString()).ToArray();
        using var request = await GetRequest(
            HttpMethod.Get,
            $"/internal/products/{IdsAsQueryString(ids, "id")}",
            cancellationToken);
        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        return await ReadResponse<GetFullProductsResponse, IReadOnlyList<InternalFullProduct>>(
            response,
            x => x.Products,
            cancellationToken);
    }

    private record GetFullProductsResponse
    {
        [JsonPropertyName("products")]
        public IReadOnlyList<InternalFullProduct> Products { get; } = [];
    }
}