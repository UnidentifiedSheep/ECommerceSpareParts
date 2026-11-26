using Integrations.Armtek.Interfaces;
using Integrations.Armtek.Models;
using Integrations.Armtek.Requests;
using Integrations.Common;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Interfaces;
using Integrations.Supplier.Models;
using Integrations.Supplier.Models.Requests;

namespace Integrations.Armtek;

public class ArmtekSupplier(
	IArmtekClient client,
	IConnectionProvider<ArmtekConnection> connectionProvider,
	TimeProvider timeProvider) : ISupplier
{
	private const string DefaultCurrency = "RUB";

	private const int DefaultDeliveryProbability = 99;

	public Enums.Supplier Supplier => Enums.Supplier.Armtek;

	public async Task<Response<IReadOnlyList<SupplierProduct>>> GetProductsAsync(
		GetProductsRequest request,
		CancellationToken cancellationToken = default)
		=> string.IsNullOrWhiteSpace(request.Brand)
			? await GetAssortmentAsync(request, cancellationToken)
			: await GetOffersAsync(request, request.Brand, cancellationToken);

	public async Task<ConnectionCheck> CheckConnectionAsync(CancellationToken cancellationToken = default)
		=> await connectionProvider.CheckConnectionAsync(cancellationToken);

	private async Task<Response<IReadOnlyList<SupplierProduct>>> GetAssortmentAsync(
		GetProductsRequest request,
		CancellationToken cancellationToken)
	{
		var response = await client.AssortmentSearchAsync(
			new AssortmentSearchRequest
			{
				Number = request.Number
			},
			cancellationToken);

		if (!response.Success)
			return Response<IReadOnlyList<SupplierProduct>>.FromFail(response);

		return Response<IReadOnlyList<SupplierProduct>>.Ok(
			response
				.ValueOrThrow
				.Where(x => !string.IsNullOrWhiteSpace(x.Number) && !string.IsNullOrWhiteSpace(x.Brand))
				.Select(x => new SupplierProduct
				{
					Id = string.Empty,
					Number = x.Number!.Trim(),
					Brand = x.Brand!.Trim(),
					Names = string.IsNullOrWhiteSpace(x.Name) ? [] : [x.Name.Trim()],
					Analogues = [],
					Positions = []
				})
				.ToList());
	}

	private async Task<Response<IReadOnlyList<SupplierProduct>>> GetOffersAsync(
		GetProductsRequest request,
		string brand,
		CancellationToken cancellationToken)
	{
		var response = await client.SearchAsync(
			new SearchRequest
			{
				Number = request.Number,
				Brand = brand,
				WithAnalogues = request.ShowAnalogues,
				StorageCode = request.StorageCode
			},
			cancellationToken);

		if (!response.Success)
			return Response<IReadOnlyList<SupplierProduct>>.FromFail(response);

		var endOfDay = ArmtekTime.EndOfDayUtc(timeProvider.GetUtcNow());
		var items = response
			.ValueOrThrow
			.Where(IsValid)
			.ToLookup(x => IsRequested(x, brand, request.Number));
		var requested = items[true].ToList();

		var analogues = items[false]
			.GroupBy(x => (Brand: x.Brand!.Trim().ToUpperInvariant(), Number: NormalizeNumber(x.Number!)))
			.Select(group => CreateProduct(
				group.First().Brand!.Trim(),
				group.First().Number!.Trim(),
				group.ToList(),
				[],
				endOfDay))
			.ToList();

		return Response<IReadOnlyList<SupplierProduct>>.Ok(
		[
			CreateProduct(
				brand,
				request.Number,
				requested,
				analogues,
				endOfDay)
		]);
	}

	private static SupplierProduct CreateProduct(
		string brand,
		string number,
		IReadOnlyList<SearchItem> items,
		IReadOnlyList<SupplierProduct> analogues,
		DateTime endOfDay)
		=> new()
		{
			Id = items.Select(x => x.ArticleId).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
			Brand = brand,
			Number = number,
			Names = items
				.Select(x => x.Name?.Trim())
				.OfType<string>()
				.Where(x => x.Length > 0)
				.Distinct()
				.ToList(),
			Positions = items.Select(x => CreatePosition(x, endOfDay)).ToList(),
			Analogues = analogues
		};

	private static SupplierPosition CreatePosition(SearchItem item, DateTime endOfDay)
		=> new()
		{
			Id = $"{item.ArticleId}:{item.WarehouseCode}",
			PurchaseInfo = new PurchaseInfo
			{
				AvailableQuantity = item.AvailableQuantity,
				MinimumPurchaseQuantity = item.MinimumOrderQuantity,
				QuantityCoefficient = item.PackQuantity,
				DaysToRefund = item.DaysToRefund,
				PartnerWarehouse = item.IsPartnerWarehouse,
				PriceInfo = new PriceInfo
				{
					Price = item.Price!.Value,
					CurrencyCode = string.IsNullOrWhiteSpace(item.Currency) ? DefaultCurrency : item.Currency.Trim()
				}
			},
			DeliveryInfo = item.DeliveryDate is { } deliveryDate
				? new DeliveryInfo
				{
					DeliveryDate = deliveryDate.UtcDateTime,
					GuaranteedDeliveryDate = (item.GuaranteedDeliveryDate ?? deliveryDate).UtcDateTime,
					DeliveryProbability = item.DeliveryProbability ?? DefaultDeliveryProbability,
					OrderTill = item.OrderDeadline?.UtcDateTime ?? endOfDay
				}
				: null
		};

	private static bool IsValid(SearchItem item)
		=> !string.IsNullOrWhiteSpace(item.Number) && !string.IsNullOrWhiteSpace(item.Brand) && item.Price > 0;

	private static bool IsRequested(SearchItem item, string brand, string number)
		=> !item.IsAnalog &&
			string.Equals(
				item.Brand!.Trim(),
				brand.Trim(),
				StringComparison.OrdinalIgnoreCase) &&
			NormalizeNumber(item.Number!) == NormalizeNumber(number);

	private static string NormalizeNumber(string value)
		=> new(value.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
}
