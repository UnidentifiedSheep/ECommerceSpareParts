using System.Net;
using Integrations.Armtek;
using Integrations.Armtek.Interfaces;
using Integrations.Armtek.Models;
using Integrations.Armtek.Requests;
using Integrations.Common;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Models.Requests;

namespace Infrastructure.Tests.Integrations;

public sealed class ArmtekSupplierTests
{
	private static readonly DateTimeOffset Now = new(2026, 11, 26, 10, 0, 0, TimeSpan.Zero);

	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	private readonly StubClient _client = new();

	[Fact]
	public async Task GetProductsAsync_WithBrand_SplitsRequestedAndAnalogues()
	{
		_client.SearchItems =
		[
			Item("A152", "MFILTER", keyzak: "MOV0006959"),
			Item("A 152", "mfilter", keyzak: "0000176139", partner: "1040088"),
			Item("81-21002-SX", "STELLOX", analog: "X", keyzak: "MOV0006959"),
			Item("81-21002-SX", "STELLOX", analog: "X", keyzak: "MOV0006930"),
			Item("A152", "MFILTER", analog: "X", keyzak: "MOV0063000")
		];

		var result = await CreateSupplier().GetProductsAsync(Request("MFILTER"), CancellationToken);

		var product = Assert.Single(result.ValueOrThrow);
		Assert.Equal("MFILTER", product.Brand);
		Assert.Equal("A152", product.Number);
		Assert.Equal(["249337:MOV0006959", "249337:0000176139"], product.Positions.Select(x => x.Id));
		Assert.Equal(2, product.Analogues.Count);
		Assert.Equal(2, product.Analogues.Single(x => x.Brand == "STELLOX").Positions.Count);
		Assert.Single(product.Analogues.Single(x => x.Brand == "MFILTER").Positions);
	}

	[Fact]
	public async Task GetProductsAsync_SendsSearchRequest()
	{
		await CreateSupplier().GetProductsAsync(Request("MFILTER"), CancellationToken);

		var request = Assert.IsType<SearchRequest>(_client.LastSearchRequest);
		Assert.Equal("A152", request.Number);
		Assert.Equal("MFILTER", request.Brand);
		Assert.True(request.WithAnalogues);
		Assert.Equal("MAIN-01", request.StorageCode);
	}

	[Fact]
	public async Task GetProductsAsync_MapsPosition()
	{
		_client.SearchItems =
		[
			Item(
				"A152",
				"MFILTER",
				partner: "1040088",
				delivery: "20261130120000",
				guaranteed: "20261202120000",
				orderDeadline: "20261127114500")
		];

		var position = Assert.Single(
			Assert.Single((await CreateSupplier().GetProductsAsync(Request("MFILTER"), CancellationToken))
				.ValueOrThrow).Positions);

		var purchase = position.PurchaseInfo!;
		Assert.Equal(4, purchase.AvailableQuantity);
		Assert.Equal(1, purchase.MinimumPurchaseQuantity);
		Assert.Equal(2, purchase.QuantityCoefficient);
		Assert.Equal(14, purchase.DaysToRefund);
		Assert.True(purchase.PartnerWarehouse);
		Assert.Equal(2957.00m, purchase.PriceInfo.Price);
		Assert.Equal("RUB", purchase.PriceInfo.CurrencyCode);

		var delivery = position.DeliveryInfo!;
		Assert.Equal(new DateTime(2026, 11, 30, 9, 0, 0, DateTimeKind.Utc), delivery.DeliveryDate);
		Assert.Equal(new DateTime(2026, 12, 2, 9, 0, 0, DateTimeKind.Utc), delivery.GuaranteedDeliveryDate);
		Assert.Equal(95, delivery.DeliveryProbability);
		Assert.Equal(new DateTime(2026, 11, 27, 8, 45, 0, DateTimeKind.Utc), delivery.OrderTill);
	}

	[Fact]
	public async Task GetProductsAsync_WithoutOptionalDates_UsesFallbacks()
	{
		_client.SearchItems = [Item("A152", "MFILTER", probability: "", delivery: "20261130120000")];

		var delivery = Assert.Single(
			Assert.Single((await CreateSupplier().GetProductsAsync(Request("MFILTER"), CancellationToken))
				.ValueOrThrow).Positions).DeliveryInfo!;

		Assert.Equal(delivery.DeliveryDate, delivery.GuaranteedDeliveryDate);
		Assert.Equal(99, delivery.DeliveryProbability);
		Assert.Equal(new DateTime(2026, 11, 26, 20, 59, 59, DateTimeKind.Utc), delivery.OrderTill);
	}

	[Fact]
	public async Task GetProductsAsync_WithoutDeliveryDate_ReturnsPositionWithoutDeliveryInfo()
	{
		_client.SearchItems = [Item("A152", "MFILTER", delivery: "")];

		var position = Assert.Single(
			Assert.Single((await CreateSupplier().GetProductsAsync(Request("MFILTER"), CancellationToken))
				.ValueOrThrow).Positions);

		Assert.Null(position.DeliveryInfo);
		Assert.NotNull(position.PurchaseInfo);
	}

	[Fact]
	public async Task GetProductsAsync_SkipsInvalidRows()
	{
		_client.SearchItems =
		[
			Item("A152", "MFILTER", price: "0.00"),
			Item("A152", "MFILTER", price: ""),
			Item("", "MFILTER"),
			Item("A152", "")
		];

		var product = Assert.Single((await CreateSupplier().GetProductsAsync(Request("MFILTER"), CancellationToken))
			.ValueOrThrow);

		Assert.Empty(product.Positions);
		Assert.Empty(product.Analogues);
	}

	[Fact]
	public async Task GetProductsAsync_WhenClientFails_ReturnsFail()
	{
		_client.SearchResponse = Response<IReadOnlyList<SearchItem>>.Fail(
			HttpStatusCode.UnprocessableEntity,
			"Armtek storage binding not found for 'MAIN-01'");

		var result = await CreateSupplier().GetProductsAsync(Request("MFILTER"), CancellationToken);

		Assert.False(result.Success);
		Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
		Assert.Equal("Armtek storage binding not found for 'MAIN-01'", result.Error);
	}

	[Fact]
	public async Task GetProductsAsync_WithoutBrand_ReturnsAssortment()
	{
		_client.AssortmentItems =
		[
			new AssortmentItem
			{
				Number = "A152", Brand = "MFILTER", Name = "Фильтр"
			},
			new AssortmentItem
			{
				Number = "A152", Brand = ""
			}
		];

		var result = await CreateSupplier().GetProductsAsync(Request(null), CancellationToken);

		var product = Assert.Single(result.ValueOrThrow);
		Assert.Equal("MFILTER", product.Brand);
		Assert.Equal(["Фильтр"], product.Names);
		Assert.Empty(product.Positions);
		Assert.Null(_client.LastSearchRequest);
	}

	private ArmtekSupplier CreateSupplier()
		=> new(_client, new StubConnectionProvider(), new FixedTimeProvider(Now));

	private static GetProductsRequest Request(string? brand)
		=> new()
		{
			StorageCode = "MAIN-01",
			Number = "A152",
			Brand = brand,
			ShowAnalogues = true
		};

	private static SearchItem Item(
		string number,
		string brand,
		string analog = "",
		string keyzak = "MOV0006959",
		string partner = "0",
		string price = "2957.00",
		string probability = "95",
		string delivery = "20261130120000",
		string guaranteed = "",
		string orderDeadline = "")
		=> new()
		{
			ArticleId = "249337",
			Number = number,
			Brand = brand,
			Name = "Фильтр воздушный",
			PartnerCode = partner,
			WarehouseCode = keyzak,
			AvailableQuantityRaw = "4",
			DaysToRefundRaw = "14",
			PackQuantityRaw = "2",
			MinimumOrderQuantityRaw = "1",
			DeliveryProbabilityRaw = probability,
			PriceRaw = price,
			Currency = "RUB",
			DeliveryDateRaw = delivery,
			GuaranteedDeliveryDateRaw = guaranteed,
			OrderDeadlineRaw = orderDeadline,
			AnalogFlag = analog
		};

	private sealed class StubClient : IArmtekClient
	{
		public IReadOnlyList<SearchItem> SearchItems { get; set; } = [];

		public Response<IReadOnlyList<SearchItem>>? SearchResponse { get; set; }

		public IReadOnlyList<AssortmentItem> AssortmentItems { get; set; } = [];

		public SearchRequest? LastSearchRequest { get; private set; }

		public Task<Response<IReadOnlyList<SearchItem>>> SearchAsync(
			SearchRequest request,
			CancellationToken token = default)
		{
			LastSearchRequest = request;
			return Task.FromResult(SearchResponse ?? Response<IReadOnlyList<SearchItem>>.Ok(SearchItems));
		}

		public Task<Response<IReadOnlyList<AssortmentItem>>> AssortmentSearchAsync(
			AssortmentSearchRequest request,
			CancellationToken token = default)
			=> Task.FromResult(Response<IReadOnlyList<AssortmentItem>>.Ok(AssortmentItems));
	}

	private sealed class StubConnectionProvider : IConnectionProvider<ArmtekConnection>
	{
		public Task<ArmtekConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<ConnectionCheck<ArmtekConnection>> CheckConnectionAsync(
			CancellationToken cancellationToken = default)
			=> Task.FromResult(new ConnectionCheck<ArmtekConnection>(false, null));

		async Task<ConnectionCheck> IConnectionProvider.CheckConnectionAsync(CancellationToken cancellationToken)
			=> await CheckConnectionAsync(cancellationToken);
	}

	private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
	{
		public override DateTimeOffset GetUtcNow() => now;
	}
}
