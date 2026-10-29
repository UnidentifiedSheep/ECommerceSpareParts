using System.Net;
using Enums;
using FluentAssertions;
using Integrations.Common;
using Integrations.Supplier.Models;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main.Product;
using Moq;
using Pricing.Application.Handlers.Pricing;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Entities.Offers;

namespace Pricing.Integration.Tests.HandlerTests.Pricing;

public sealed class ApplySupplierProductsTests
{
	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	[Fact]
	public async Task Handle_UsesListContractAndSkipsUnresolvedProducts()
	{
		var handler = CreateHandler(Response<IReadOnlyList<InternalResolvedProductReference>>.Ok([
			Resolved("Brand A", "ABC-123", 10), Resolved("Brand B", "ABC-456", null)
		]), out var node, out var refresh);
		var position = new SupplierPosition { Id = "position" };

		await handler.Handle(Command([
			Product("Brand A", "ABC-123", [position]), Product("Brand B", "ABC-456")
		]), CancellationToken);

		var references = (IReadOnlyCollection<InternalProductReference>)node.Invocations.Should().ContainSingle().Which.Arguments[0];
		references.Select(reference => reference.ProducerName).Should().Equal("Brand A", "Brand B");
		references.Select(reference => reference.Sku).Should().Equal("ABC-123", "ABC-456");
		references.Should().OnlyContain(reference => reference.Supplier == Supplier.Armtek);
		var positions = RefreshInput(refresh);
		positions.Keys.Should().Equal(10);
		positions[10].Should().Equal(position);
	}

	[Fact]
	public async Task Handle_AliasesResolvingToSameProduct_CombinePositionsWithoutDuplicateOffers()
	{
		var handler = CreateHandler(Response<IReadOnlyList<InternalResolvedProductReference>>.Ok([
			Resolved("Brand", "ABC-123", 10), Resolved("Brand alias", "ABC-123", 10)
		]), out _, out var refresh);
		var first = new SupplierPosition { Id = "first" };
		var second = new SupplierPosition { Id = "second" };

		await handler.Handle(Command([
			Product("Brand", "ABC-123", [first]), Product("Brand alias", "ABC-123", [first, second])
		]), CancellationToken);

		var positions = RefreshInput(refresh);
		positions.Keys.Should().Equal(10);
		positions[10].Should().Equal(first, second);
	}

	[Fact]
	public async Task Handle_IncludesAnaloguesInResolution()
	{
		var handler = CreateHandler(Response<IReadOnlyList<InternalResolvedProductReference>>.Ok([
			Resolved("Brand", "ABC-123", 10), Resolved("Analogue", "XYZ-123", 20)
		]), out var node, out var refresh);
		var position = new SupplierPosition { Id = "analogue-position" };

		await handler.Handle(Command([
			Product("Brand", "ABC-123", analogues: [Product("Analogue", "XYZ-123", [position])])
		]), CancellationToken);

		var references = (IReadOnlyCollection<InternalProductReference>)node.Invocations.Should().ContainSingle().Which.Arguments[0];
		references.Select(reference => reference.ProducerName).Should().Equal("Brand", "Analogue");
		RefreshInput(refresh)[20].Should().Equal(position);
	}

	[Fact]
	public async Task Handle_FailedResolution_DoesNotRefreshOffers()
	{
		var handler = CreateHandler(Response<IReadOnlyList<InternalResolvedProductReference>>.Fail(HttpStatusCode.ServiceUnavailable),
			out _, out var refresh);

		await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(Command([Product("Brand", "ABC-123")]), CancellationToken));

		refresh.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task Handle_EmptyProducts_DoesNotResolveOrRefresh()
	{
		var handler = CreateHandler(Response<IReadOnlyList<InternalResolvedProductReference>>.Ok([]), out var node, out var refresh);

		await handler.Handle(Command([]), CancellationToken);

		node.VerifyNoOtherCalls();
		refresh.VerifyNoOtherCalls();
	}

	private static ApplySupplierProductsHandler CreateHandler(
		Response<IReadOnlyList<InternalResolvedProductReference>> response,
		out Mock<IProductNode> node, out Mock<IOfferRefreshService> refresh)
	{
		node = new Mock<IProductNode>();
		node.Setup(client => client.ResolveProductReferences(It.IsAny<IReadOnlyCollection<InternalProductReference>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(response);
		var mainClient = new Mock<IMainClient>();
		mainClient.SetupGet(client => client.ProductNode).Returns(node.Object);
		refresh = new Mock<IOfferRefreshService>();
		refresh.Setup(service => service.RefreshOffersAsync(It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<Supplier>(),
			It.IsAny<IReadOnlyDictionary<int, IReadOnlyList<SupplierPosition>>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(Array.Empty<PriceOffer>());
		return new ApplySupplierProductsHandler(refresh.Object, mainClient.Object);
	}

	private static IReadOnlyDictionary<int, IReadOnlyList<SupplierPosition>> RefreshInput(Mock<IOfferRefreshService> refresh) =>
		(IReadOnlyDictionary<int, IReadOnlyList<SupplierPosition>>)refresh.Invocations.Should().ContainSingle().Which.Arguments[3];

	private static ApplySupplierProductsCommand Command(IReadOnlyList<SupplierProduct> products) =>
		new(DateTime.UtcNow, Supplier.Armtek, "main", products);

	private static SupplierProduct Product(string brand, string sku,
		IReadOnlyList<SupplierPosition>? positions = null, IReadOnlyList<SupplierProduct>? analogues = null) => new()
	{
		Id = brand + sku, Brand = brand, Number = sku, Names = [], Positions = positions ?? [], Analogues = analogues ?? []
	};

	private static InternalResolvedProductReference Resolved(string brand, string sku, int? productId) => new()
	{
		ProducerName = brand, Sku = sku, Supplier = Supplier.Armtek, ProductId = productId
	};
}
