using System.Net;
using System.Text;
using Abstractions.Models.Options;
using Enums;
using Integrations.Armtek;
using Integrations.Armtek.Requests;
using Integrations.Common;
using Integrations.Supplier.Connections;

namespace Infrastructure.Tests.Integrations;

public sealed class ArmtekClientTests
{
	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	private static readonly ArmtekConnection Connection = new()
	{
		BaseUrl = "https://ws.armtek.ru",
		Login = "user",
		Password = "secret",
		SalesOrganization = "4000",
		CustomerCode = "0000123456",
		ContractCode = "0000000777",
		Storages =
		[
			new ArmtekStorageConnection
			{
				StorageCode = "MAIN-01", DeliveryMode = DeliveryMode.Pickup, PointCode = "0000001234"
			},
			new ArmtekStorageConnection
			{
				StorageCode = "MAIN-02", DeliveryMode = DeliveryMode.Delivery, PointCode = "0000005678"
			}
		]
	};

	[Fact]
	public async Task SearchAsync_SendsFormWithBasicAuth()
	{
		var handler = new StubHandler("""{ "STATUS": 200, "MESSAGES": [], "RESP": [] }""");

		await CreateClient(handler).SearchAsync(
			new SearchRequest
			{
				Number = "A152",
				Brand = "MFILTER",
				WithAnalogues = true,
				StorageCode = "MAIN-01"
			},
			CancellationToken);

		Assert.Equal(HttpMethod.Post, handler.Method);
		Assert.Equal("https://ws.armtek.ru/api/ws_search/search?format=json", handler.Uri);
		Assert.Equal("Basic", handler.AuthScheme);
		Assert.Equal("user:secret", Encoding.UTF8.GetString(Convert.FromBase64String(handler.AuthParameter!)));
		Assert.Equal("application/x-www-form-urlencoded; charset=utf-8", handler.ContentType);
		Assert.Equal(
			new Dictionary<string, string>
			{
				["VKORG"] = "4000",
				["KUNNR_RG"] = "0000123456",
				["PIN"] = "A152",
				["QUERY_TYPE"] = "2",
				["KUNNR_ZA"] = "0000001234",
				["INCOTERMS"] = "1",
				["BRAND"] = "MFILTER",
				["VBELN"] = "0000000777",
				["format"] = "json"
			},
			handler.Form);
	}

	[Fact]
	public async Task SearchAsync_ParsesItems()
	{
		const string json = """
							{
							  "STATUS": 200,
							  "MESSAGES": [],
							  "RESP": [
							    {
							      "ARTID": "249337",
							      "PIN": "A152",
							      "BRAND": "MFILTER",
							      "NAME": "Фильтр воздушный",
							      "PARNR": "1040088",
							      "KEYZAK": "0000176139",
							      "RVALUE": "4",
							      "RETDAYS": 14,
							      "RDPRF": "2",
							      "MINBM": "1",
							      "VENSL": "95",
							      "PRICE": "2957.00",
							      "WAERS": "RUB",
							      "DLVDT": "20270120120000",
							      "WRNTDT": "",
							      "ANALOG": ""
							    }
							  ]
							}
							""";

		var result = await CreateClient(new StubHandler(json)).SearchAsync(SearchRequest(), CancellationToken);

		Assert.True(result.Success, result.Error);
		var item = Assert.Single(result.ValueOrThrow);
		Assert.Equal("A152", item.Number);
		Assert.Equal(4, item.AvailableQuantity);
		Assert.Equal(14, item.DaysToRefund);
		Assert.Equal(2, item.PackQuantity);
		Assert.Equal(95, item.DeliveryProbability);
		Assert.Equal(2957.00m, item.Price);
		Assert.True(item.IsPartnerWarehouse);
		Assert.Equal(new DateTimeOffset(2027, 1, 20, 12, 0, 0, TimeSpan.FromHours(3)), item.DeliveryDate);
		Assert.Null(item.GuaranteedDeliveryDate);
	}

	[Fact]
	public async Task SearchAsync_WhenResponseHasError_ReturnsFail()
	{
		const string json = """
							{
							  "STATUS": 200,
							  "MESSAGES": [ { "TYPE": "E", "TEXT": "Покупатель не найден", "DATE": "" } ],
							  "RESP": []
							}
							""";

		var result = await CreateClient(new StubHandler(json)).SearchAsync(SearchRequest(), CancellationToken);

		Assert.False(result.Success);
		Assert.Equal("Покупатель не найден", result.Error);
	}

	[Fact]
	public async Task SearchAsync_WhenStatusIsNotSuccess_ReturnsFail()
	{
		var result = await CreateClient(new StubHandler("""{ "STATUS": 401, "MESSAGES": [] }"""))
			.SearchAsync(SearchRequest(), CancellationToken);

		Assert.False(result.Success);
		Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
	}

	[Fact]
	public async Task SearchAsync_WhenRespIsNotArray_ReturnsEmpty()
	{
		var result = await CreateClient(new StubHandler("""{ "STATUS": 200, "MESSAGES": [], "RESP": {} }"""))
			.SearchAsync(SearchRequest(), CancellationToken);

		Assert.True(result.Success, result.Error);
		Assert.Empty(result.ValueOrThrow);
	}

	[Theory]
	[InlineData("""{ "ARRAY": [ { "PIN": "A152" }, { "PIN": "A153" } ] }""", 2)]
	[InlineData("""{ "ARRAY": { "PIN": "A152" } }""", 1)]
	[InlineData("""[ { "PIN": "A152" } ]""", 1)]
	public async Task SearchAsync_ReadsSupportedRespShapes(string resp, int expectedCount)
	{
		var json = $$"""{ "STATUS": 200, "MESSAGES": [], "RESP": {{resp}} }""";

		var result = await CreateClient(new StubHandler(json)).SearchAsync(SearchRequest(), CancellationToken);

		Assert.True(result.Success, result.Error);
		Assert.Equal(expectedCount, result.ValueOrThrow.Count);
	}

	[Theory]
	[InlineData(">20", 20)]
	[InlineData(">=5", 5)]
	[InlineData("≥ 3", 3)]
	[InlineData("12", 12)]
	public async Task SearchAsync_ParsesQuantityWithPrefix(string rvalue, int expected)
	{
		var json = $$"""{ "STATUS": 200, "MESSAGES": [], "RESP": [ { "RVALUE": "{{rvalue}}", "PRICE": "1 234,50", "ANALOG": "X" } ] }""";

		var item = Assert.Single((await CreateClient(new StubHandler(json)).SearchAsync(SearchRequest(), CancellationToken)).ValueOrThrow);

		Assert.Equal(expected, item.AvailableQuantity);
		Assert.Equal(1234.50m, item.Price);
		Assert.True(item.IsAnalog);
	}

	[Fact]
	public async Task AssortmentSearchAsync_SendsSalesOrganizationAndNumber()
	{
		const string json = """
							{ "STATUS": 200, "MESSAGES": [], "RESP": [ { "PIN": "A152", "BRAND": "MFILTER", "NAME": "Фильтр" } ] }
							""";
		var handler = new StubHandler(json);

		var result = await CreateClient(handler).AssortmentSearchAsync(
			new AssortmentSearchRequest
			{
				Number = "A152"
			},
			CancellationToken);

		Assert.Equal("https://ws.armtek.ru/api/ws_search/assortment_search?format=json", handler.Uri);
		Assert.Equal(
			new Dictionary<string, string>
			{
				["VKORG"] = "4000", ["PIN"] = "A152", ["format"] = "json"
			},
			handler.Form);
		Assert.Equal("MFILTER", Assert.Single(result.ValueOrThrow).Brand);
	}

	[Fact]
	public async Task SearchAsync_WithDeliveryStorage_SendsDeliveryPoint()
	{
		var handler = new StubHandler("""{ "STATUS": 200, "MESSAGES": [], "RESP": [] }""");

		await CreateClient(handler).SearchAsync(SearchRequest(" main-02 "), CancellationToken);

		Assert.Equal("0000005678", handler.Form["KUNNR_ZA"]);
		Assert.Equal("0", handler.Form["INCOTERMS"]);
	}

	[Fact]
	public async Task SearchAsync_WithUnboundStorage_ReturnsFailWithoutRequest()
	{
		var handler = new StubHandler("""{ "STATUS": 200, "MESSAGES": [], "RESP": [] }""");

		var result = await CreateClient(handler).SearchAsync(SearchRequest("UNKNOWN"), CancellationToken);

		Assert.False(result.Success);
		Assert.Contains("UNKNOWN", result.Error);
		Assert.Null(handler.Method);
	}

	private static SearchRequest SearchRequest(string storageCode = "MAIN-02")
		=> new()
		{
			Number = "A152",
			StorageCode = storageCode
		};

	private static ArmtekClient CreateClient(StubHandler handler)
		=> new(new HttpClient(handler), new StubConnectionProvider(), new ProjectJsonOptions());

	private sealed class StubConnectionProvider : IConnectionProvider<ArmtekConnection>
	{
		public Task<ArmtekConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult(Connection);

		public Task<ConnectionCheck<ArmtekConnection>> CheckConnectionAsync(
			CancellationToken cancellationToken = default)
			=> Task.FromResult(new ConnectionCheck<ArmtekConnection>(true, Connection));

		async Task<ConnectionCheck> IConnectionProvider.CheckConnectionAsync(CancellationToken cancellationToken)
			=> await CheckConnectionAsync(cancellationToken);
	}

	private sealed class StubHandler(string responseJson) : HttpMessageHandler
	{
		public HttpMethod? Method { get; private set; }

		public string? Uri { get; private set; }

		public string? AuthScheme { get; private set; }

		public string? AuthParameter { get; private set; }

		public string? ContentType { get; private set; }

		public Dictionary<string, string> Form { get; private set; } = [];

		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			Method = request.Method;
			Uri = request.RequestUri?.ToString();
			AuthScheme = request.Headers.Authorization?.Scheme;
			AuthParameter = request.Headers.Authorization?.Parameter;
			ContentType = request.Content?.Headers.ContentType?.ToString();

			var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
			Form = body
				.Split('&', StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Split('='))
				.ToDictionary(x => System.Uri.UnescapeDataString(x[0]), x => System.Uri.UnescapeDataString(x[1].Replace('+', ' ')));

			return new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
			};
		}
	}
}
