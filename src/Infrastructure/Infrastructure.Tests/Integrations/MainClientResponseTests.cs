using System.Net;
using System.Text;
using Abstractions.Models.Options;
using Enums;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Models.Main.Product;
using Internal.Integration.Main;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace Infrastructure.Tests.Integrations;

public sealed class MainClientResponseTests
{
	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	[Fact]
	public async Task GetFullProducer_ReadsProducersFromResponse()
	{
		var client = CreateClient(
			"""
			{ "producers": [ { "id": 7, "name": "Bosch", "description": null, "aliases": ["BOSCH AG"] } ] }
			""");

		var response = await client.ProducerNode.GetFullProducer([7], CancellationToken);

		var producer = Assert.Single(response.ValueOrThrow);
		Assert.Equal(7, producer.Id);
		Assert.Equal("Bosch", producer.Name);
		Assert.Equal(["BOSCH AG"], producer.Aliases);
	}

	[Fact]
	public async Task GetSupplierProductReferences_ReadsProductsFromResponse()
	{
		var client = CreateClient(
			"""
			{ "products": [ { "productId": 3, "sku": "A152", "supplierProducerName": "MFILTER" } ] }
			""");

		var response = await client.ProductNode.GetSupplierProductReferences([3], Supplier.Armtek, CancellationToken);

		var product = Assert.Single(response.ValueOrThrow);
		Assert.Equal(3, product.ProductId);
		Assert.Equal("A152", product.Sku);
		Assert.Equal("MFILTER", product.SupplierProducerName);
	}

	[Fact]
	public async Task ResolveProductReferences_ReadsProductsFromResponse()
	{
		var client = CreateClient(
			"""
			{ "products": [ { "producerId": 7, "productId": 3, "sku": "A152", "producerName": "MFILTER", "supplier": "Armtek" } ] }
			""");

		var response = await client.ProductNode.ResolveProductReferences(
			[new InternalProductReference { Sku = "A152", ProducerName = "MFILTER" }],
			CancellationToken);

		var product = Assert.Single(response.ValueOrThrow);
		Assert.Equal(7, product.ProducerId);
		Assert.Equal(3, product.ProductId);
		Assert.Equal(Supplier.Armtek, product.Supplier);
	}

	private static RootClient CreateClient(string responseJson)
	{
		var authClient = new Mock<IAuthClient>();
		authClient
			.Setup(client => client.GetAuthToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync("token");
		var credentials = new Mock<IOptionsMonitor<InternalServiceCredentials>>();
		credentials
			.Setup(options => options.CurrentValue)
			.Returns(new InternalServiceCredentials { Service = "search", Secret = "secret" });
		var handler = new Mock<HttpMessageHandler>();
		handler
			.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
			});
		var httpClient = new HttpClient(handler.Object)
		{
			BaseAddress = new Uri("http://main")
		};

		return new RootClient(httpClient, authClient.Object, credentials.Object, new ProjectJsonOptions());
	}
}
