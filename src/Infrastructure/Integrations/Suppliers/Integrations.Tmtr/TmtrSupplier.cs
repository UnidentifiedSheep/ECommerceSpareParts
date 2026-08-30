using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using Integrations.Common;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Interfaces;
using Integrations.Supplier.Models;
using Integrations.Supplier.Settings;
using Integrations.Tmtr.Enums;
using Integrations.Tmtr.Models;
using Integrations.Tmtr.Requests;
using GetProductsRequest = Integrations.Supplier.Models.Requests.GetProductsRequest;

namespace Integrations.Tmtr;

public class TmtrSupplier(
	ITmtrClient client,
	ISupplierSettingsProvider<TmtrSettings> settingsProvider,
	IConnectionProvider<TmtrConnection> connectionProvider) : ISupplier
{
	public global::Enums.Supplier Supplier => global::Enums.Supplier.Tmtr;

	public async Task<Response<IReadOnlyList<SupplierProduct>>> GetProductsAsync(
		GetProductsRequest request,
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(request.Brand))
			return await GetProducts(request, cancellationToken);
		return await GetPrices(request, cancellationToken);
	}

	public async Task<ConnectionCheck> CheckConnectionAsync(CancellationToken cancellationToken = default) =>
		await connectionProvider.CheckConnectionAsync(cancellationToken);

	private async Task<Response<IReadOnlyList<SupplierProduct>>> GetProducts(
		GetProductsRequest request,
		CancellationToken cancellationToken = default)
	{
		var response = await client.GetProductsAsync(
			new Requests.GetProductsRequest
			{
				Number = request.Number
			},
			cancellationToken);

		if (IsFail(response, out var failResponse))
			return failResponse;

		return Response<IReadOnlyList<SupplierProduct>>.Ok(
			response
				.ValueOrThrow
				.Select(x => new SupplierProduct
				{
					Analogues = [],
					Brand = x.Brand,
					Id = string.Empty,
					Number = x.Number,
					Names = [x.Name ?? string.Empty],
					Positions = []
				})
				.ToList());
	}

	private async Task<Response<IReadOnlyList<SupplierProduct>>> GetPrices(
		GetProductsRequest request,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request.Brand);
		var response = await client.GetPricesAsync(
			new GetPricesRequest
			{
				Brand = request.Brand, Number = request.Number
			},
			cancellationToken);

		if (IsFail(response, out var failResponse))
			return failResponse;

		var setting = await settingsProvider.GetSettingsAsync(cancellationToken);
		var requestedPositions = new List<GetPriceItem>();
		var analogues = new Dictionary<(string brand, string number), List<GetPriceItem>>();

		foreach (var item in response.ValueOrThrow)
		{
			if (IsRequestedProduct(
					request.Brand,
					request.Number,
					item.Brand,
					item.Number))
			{
				requestedPositions.Add(item);
				continue;
			}

			var key = (item.Brand, item.Number);
			if (!analogues.TryGetValue(key, out var positions))
				analogues[key] = positions = [];

			positions.Add(item);
		}

		var firstPositionOrDefault = requestedPositions.FirstOrDefault();

		return Response<IReadOnlyList<SupplierProduct>>.Ok(
		[
			new SupplierProduct
			{
				Id = firstPositionOrDefault?.ProductId.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
				Brand = request.Brand,
				Number = request.Number,
				Names = requestedPositions.Select(x => x.ProductName).ToList(),
				Positions = requestedPositions.Select(x => AdaptToPosition(x, setting)).ToList(),
				Analogues = analogues
					.Select(x => new SupplierProduct
					{
						Id = x.Value.First().ProductId.ToString(CultureInfo.InvariantCulture),
						Brand = x.Key.brand,
						Number = x.Key.number,
						Names = x.Value.Select(z => z.ProductName).ToList(),
						Analogues = [],
						Positions = x.Value.Select(z => AdaptToPosition(z, setting)).ToList()
					})
					.ToList()
			}
		]);
	}

	private static SupplierPosition AdaptToPosition(GetPriceItem item, TmtrSettings settings) => new()
	{
		Id = CreateSourceKey(item),
		DeliveryInfo = item.ExpectedDeliveryDate.HasValue
			? new DeliveryInfo
			{
				DeliveryDate = item.ExpectedDeliveryDate.Value.UtcDateTime,
				DeliveryProbability = 99,
				GuaranteedDeliveryDate =
					item.GuaranteedDeliveryDate?.UtcDateTime ?? item.ExpectedDeliveryDate.Value
						.AddDays(settings.GuaranteedDeliveryOffsetDays)
						.UtcDateTime,
				OrderTill = DateTime.UtcNow.Date.AddHours(14) //The order can be placed till 14 utc.
			}
			: null,
		PurchaseInfo = new PurchaseInfo
		{
			AvailableQuantity = item.AvailableQuantity,
			MinimumPurchaseQuantity = item.MinimumOrderQuantity,
			QuantityCoefficient = item.MinimumPackQuantity,
			PartnerWarehouse = item.LocationType == OfferLocationType.PartnerNetworkWarehouse,
			DaysToRefund = 14, //14 days to refund
			PriceInfo = new PriceInfo
			{
				CurrencyCode = "RUB", Price = item.UnitPrice
			}
		}
	};

	private static string CreateSourceKey(GetPriceItem item)
	{
		return $"{item.ProductId}:" + $"{item.WarehouseIdentifier}:" + $"{item.StorageLocationCode}:" +
			$"{item.PriceListId}:" + $"{item.PriceListItemId}:" + $"{item.LocationType}";
	}

	private static bool IsRequestedProduct(
		string requestedBrand,
		string requestedSku,
		string compareBrand,
		string compareSku)
	{
		if (!IsSameBrand(compareBrand, requestedBrand))
			return false;

		if (string.Equals(
				compareSku.Trim(),
				requestedSku.Trim(),
				StringComparison.OrdinalIgnoreCase))
			return true;

		return NormalizeSku(requestedSku) == NormalizeSku(compareSku);
	}

	private static bool IsSameBrand(string brand1, string brand2) => string.Equals(
		brand1,
		brand2,
		StringComparison.OrdinalIgnoreCase);
	private static string NormalizeSku(string value) => new(
		value.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

	private static bool IsFail<T>(
		Response<T> response,
		[NotNullWhen(true)] out Response<IReadOnlyList<SupplierProduct>>? failResponse)
	{
		failResponse = response.Success
			? null
			: Response<IReadOnlyList<SupplierProduct>>.Fail(
				response.StatusCode ?? HttpStatusCode.InternalServerError,
				response.Error);

		return !response.Success;
	}
}
