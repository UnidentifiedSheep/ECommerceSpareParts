using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Abstractions.Models.Options;
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
	IOptionsMonitor<InternalServiceCredentials> optionsMonitor,
	ProjectJsonOptions jsonOptions) : InternalClientBase(
		authClient,
		optionsMonitor,
		jsonOptions),
	IProductNode
{
	public async Task<Response<IReadOnlyList<InternalFullProduct>>> GetFullProduct(
		IEnumerable<int> productIds,
		CancellationToken cancellationToken = default)
	{
		var ids = productIds.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();
		using var request = await GetRequest(
			HttpMethod.Get,
			$"/internal/products/{IdsAsQueryString(ids, "id")}",
			cancellationToken);
		using var response = await httpClient.SendAsync(request, cancellationToken);

		return await ReadResponse<GetFullProductsResponse, IReadOnlyList<InternalFullProduct>>(
			response,
			x => x.Products,
			cancellationToken);
	}

	public async Task<Response<IReadOnlyList<InternalSupplierProductResolvedReference>>>
		GetSupplierProductReferences(
			IEnumerable<int> productIds,
			Supplier supplier,
			CancellationToken cancellationToken = default)
	{
		var ids = productIds.Distinct().Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();
		var idsQueryString = IdsAsQueryString(ids, "id");
		var supplierSeparator = idsQueryString.Length == 0 ? "?" : "&";
		var supplierQueryString = $"{supplierSeparator}supplier={Uri.EscapeDataString(supplier.ToString())}";

		using var request = await GetRequest(
			HttpMethod.Get,
			$"/internal/products/supplier-references{idsQueryString}{supplierQueryString}",
			cancellationToken);

		using var response = await httpClient.SendAsync(request, cancellationToken);

		return await ReadResponse<
			GetSupplierProductReferencesResponse, IReadOnlyList<InternalSupplierProductResolvedReference>>(
			response,
			x => x.Products,
			cancellationToken);
	}

	public async Task<Response<IReadOnlyList<InternalResolvedProductReference>>> ResolveProductReferences(
		IReadOnlyCollection<InternalProductReference> references,
		CancellationToken cancellationToken = default)
	{
		using var request = await GetRequest(HttpMethod.Post, "/internal/products/resolve-references", cancellationToken);
		request.Content = JsonContent.Create(new ResolveProductReferencesRequest { References = references });
		using var response = await httpClient.SendAsync(request, cancellationToken);

		return await ReadResponse<ResolveProductReferencesResponse, IReadOnlyList<InternalResolvedProductReference>>(
			response, result => result.Products, cancellationToken);
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

	private record ResolveProductReferencesRequest
	{
		[JsonPropertyName("references")]
		public required IReadOnlyCollection<InternalProductReference> References { get; init; }
	}

	private record ResolveProductReferencesResponse
	{
		[JsonPropertyName("products")]
		public IReadOnlyList<InternalResolvedProductReference> Products { get; init; } = [];
	}
}
