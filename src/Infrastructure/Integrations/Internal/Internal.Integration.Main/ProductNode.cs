using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Enums;
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

    public async Task<Response<IReadOnlyList<InternalSupplierProductResolvedReference>>> GetSupplierProductReferences(
        IEnumerable<int> productIds,
        Supplier supplier,
        CancellationToken cancellationToken = default)
    {
        var ids = productIds.Distinct().Select(x => x.ToString()).ToArray();
        var idsQueryString = IdsAsQueryString(ids, "id");
        var supplierSeparator = idsQueryString.Length == 0 ? "?" : "&";
        var supplierQueryString = $"{supplierSeparator}supplier={Uri.EscapeDataString(supplier.ToString())}";

        using var request = await GetRequest(
            HttpMethod.Get,
            $"/internal/products/supplier-references{idsQueryString}{supplierQueryString}",
            cancellationToken);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        return await ReadResponse<
            GetSupplierProductReferencesResponse, 
            IReadOnlyList<InternalSupplierProductResolvedReference>>(
            response,
            x => x.Products,
            cancellationToken);
    }

    public async Task<Response<Dictionary<Supplier, IReadOnlyList<InternalSupplierProductResolvedReference>>>> ResolveSupplierProductReferences(
        Dictionary<Supplier, IEnumerable<InternalSupplierProductReferenceLookup>> references,
        CancellationToken cancellationToken = default)
    {
        using var request = await GetRequest(
            HttpMethod.Post,
            "/internal/products/resolve-supplier-references",
            cancellationToken);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        return await ReadResponse<
            ResolveSupplierProductReferencesResponse,
            Dictionary<Supplier, IReadOnlyList<InternalSupplierProductResolvedReference>>>(
            response,
            x => x.Products.ToDictionary(
                z => z.Key, 
                IReadOnlyList<InternalSupplierProductResolvedReference> (
                    z) => z.Value.ToList()
                ),
            cancellationToken);
    }

    private record GetFullProductsResponse
    {
        [JsonPropertyName("products")]
        public IReadOnlyList<InternalFullProduct> Products { get; init; } = [];
    }

    private record GetSupplierProductReferencesResponse
    {
        [JsonPropertyName("products")]
        public IReadOnlyList<InternalSupplierProductResolvedReference> Products { get; init; } = [];
    }

    private record ResolveSupplierProductReferencesRequest
    {
        [JsonPropertyName("references")]
        public required Dictionary<Supplier, IEnumerable<InternalSupplierProductReferenceLookup>> References { get; init; }
    }

    private record ResolveSupplierProductReferencesResponse
    {
        [JsonPropertyName("products")]
        public Dictionary<Supplier, IEnumerable<InternalSupplierProductResolvedReference>> Products { get; init; } = [];
    }
}
