using Abstractions.Models;
using Application.Common.Models;
using FluentAssertions;
using Main.Application.Handlers.Sales.GetProductSaleHistory;
using Main.Entities.Product;
using Tests.DataBuilders.Sale;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Sale;

namespace Tests.HandlersTests.Sales;

public class GetProductSaleHistoryTests : IntegrationTest
{
	public GetProductSaleHistoryTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<SaleTestContext>();
	}

	private SaleTestContext SaleContext => GetContext<SaleTestContext>();

	[Fact]
	public async Task GetProductSaleHistory_ReturnsCompletedSaleWithWeightedAverageBuyPrice()
	{
		var sale = SaleContext.Sale;
		var content = sale.Contents.Single();
		var expectedAverageBuyPrice = content.Details.Sum(x => x.BuyPrice * x.Count) / content.Count;

		var result = await Mediator.Send(CreateQuery(), CancellationToken);

		var history = result.History.Should().ContainSingle().Subject;
		history.SaleContentId.Should().Be(content.Id);
		history.ProductId.Should().Be(content.ProductId);
		history.OrganizationId.Should().Be(sale.OrganizationId);
		history.CurrencyId.Should().Be(sale.CurrencyId);
		history.StorageCode.Should().Be(sale.StorageCode);
		history.Quantity.Should().Be(content.Count);
		history.Discount.Should().Be(content.Discount);
		history.Price.Should().Be(content.Price);
		history.AverageBuyPrice.Should().Be(expectedAverageBuyPrice);
		history
			.SaleDate
			.Should()
			.BeCloseTo(sale.SaleDatetime.ToUniversalTime(), TimeSpan.FromMicroseconds(1));
		history.WhoCreated.Should().Be(sale.WhoCreated);
	}

	[Fact]
	public async Task GetProductSaleHistory_DoesNotReturnDraftSales()
	{
		var completedSale = SaleContext.Sale;
		var completedContent = completedSale.Contents.Single();
		var draftContent = new SaleContentBuilder(Faker)
			.WithProductId(completedContent.ProductId)
			.WithStorageContentIds(completedContent.Details.Select(x => x.StorageContentId))
			.WithCurrencyId(completedSale.CurrencyId)
			.WithCount(1)
			.WithDetailsCount(1)
			.Build();

		var draftSale = new SaleBuilder(Faker)
			.WithCurrencyId(completedSale.CurrencyId)
			.WithContents([draftContent])
			.WithUserId(completedSale.UserId)
			.WithOrganizationId(completedSale.OrganizationId)
			.WithStorageCode(completedSale.StorageCode)
			.WithTransactionId(completedSale.TransactionId)
			.Build();

		await Context.AddAsync(draftSale, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(CreateQuery(), CancellationToken);

		result.History.Should().ContainSingle().Which.SaleContentId.Should().Be(completedContent.Id);
	}

	[Fact]
	public async Task GetProductSaleHistory_AppliesOptionalFiltersAndSort()
	{
		var sale = SaleContext.Sale;

		var result = await Mediator.Send(
			CreateQuery(
				sale.StorageCode,
				sale.OrganizationId,
				sale.OrganizationId,
				sale.CurrencyId,
				["averageBuyPrice_desc"]),
			CancellationToken);

		result.History.Should().ContainSingle();
	}

	[Fact]
	public async Task GetProductSaleHistory_WhenFilterDoesNotMatch_ReturnsEmptyHistory()
	{
		var result = await Mediator.Send(CreateQuery(organizationId: Guid.NewGuid()), CancellationToken);

		result.History.Should().BeEmpty();
	}

	[Fact]
	public async Task GetProductSaleHistory_WithPreferredOrganization_ReturnsItsSalesFirstAndThenFallback()
	{
		var preferredSale = SaleContext.Sale;
		var preferredContent = preferredSale.Contents.Single();
		var fallbackOrganizationId = SaleContext.SenderBalance.OrganizationId;
		var fallbackContent = new SaleContentBuilder(Faker)
			.WithProductId(preferredContent.ProductId)
			.WithStorageContentIds(preferredContent.Details.Select(x => x.StorageContentId))
			.WithCurrencyId(preferredSale.CurrencyId)
			.WithCount(1)
			.WithDetailsCount(1)
			.Build();

		var fallbackSale = new SaleBuilder(Faker)
			.WithCurrencyId(preferredSale.CurrencyId)
			.WithContents([fallbackContent])
			.WithUserId(preferredSale.UserId)
			.WithOrganizationId(fallbackOrganizationId)
			.WithStorageCode(preferredSale.StorageCode)
			.WithTransactionId(preferredSale.TransactionId)
			.WithSaleDate(preferredSale.SaleDatetime.AddDays(1))
			.Completed()
			.Build();

		await Context.AddAsync(fallbackSale, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(
			CreateQuery(preferredOrganizationId: preferredSale.OrganizationId, sortBy: ["saleDate_desc"]),
			CancellationToken);

		result
			.History
			.Select(x => x.OrganizationId)
			.Should()
			.Equal(preferredSale.OrganizationId, fallbackOrganizationId);
	}

	[Fact]
	public async Task GetProductSaleHistory_ShowCrosses_IncludesCrossSalesInSharedPagination()
	{
		var sale = SaleContext.Sale;
		var originalContent = sale.Contents.Single();
		var products = GetContext<ProductTestContext>().Products;
		var crossProduct = products.First(x => x.Id != SaleContext.Product.Id);
		var unrelatedProduct = products.First(x => x.Id != SaleContext.Product.Id && x.Id != crossProduct.Id);

		await Context.ProductCrosses.AddAsync(
			ProductCross.Create(SaleContext.Product.Id, crossProduct.Id),
			CancellationToken);

		var crossContent = new SaleContentBuilder(Faker)
			.WithProductId(crossProduct.Id)
			.WithStorageContentIds(originalContent.Details.Select(x => x.StorageContentId))
			.WithCurrencyId(sale.CurrencyId)
			.WithCount(1)
			.WithDetailsCount(1)
			.Build();
		var unrelatedContent = new SaleContentBuilder(Faker)
			.WithProductId(unrelatedProduct.Id)
			.WithStorageContentIds(originalContent.Details.Select(x => x.StorageContentId))
			.WithCurrencyId(sale.CurrencyId)
			.WithCount(1)
			.WithDetailsCount(1)
			.Build();
		var laterSale = new SaleBuilder(Faker)
			.WithCurrencyId(sale.CurrencyId)
			.WithContents([crossContent, unrelatedContent])
			.WithUserId(sale.UserId)
			.WithOrganizationId(sale.OrganizationId)
			.WithStorageCode(sale.StorageCode)
			.WithTransactionId(sale.TransactionId)
			.WithSaleDate(sale.SaleDatetime.AddDays(1))
			.Completed()
			.Build();

		await Context.AddAsync(laterSale, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		var withoutCrosses = await Mediator.Send(CreateQuery(sortBy: ["saleDate_desc"]), CancellationToken);
		var firstPage = await Mediator.Send(
			CreateQuery(sortBy: ["saleDate_desc"], showCrosses: true, pagination: new Pagination(0, 1)),
			CancellationToken);
		var secondPage = await Mediator.Send(
			CreateQuery(sortBy: ["saleDate_desc"], showCrosses: true, pagination: new Pagination(1, 1)),
			CancellationToken);

		withoutCrosses.History.Should().ContainSingle().Which.SaleContentId.Should().Be(originalContent.Id);
		firstPage.History.Should().ContainSingle().Which.ProductId.Should().Be(crossProduct.Id);
		secondPage.History.Should().ContainSingle().Which.SaleContentId.Should().Be(originalContent.Id);
	}

	private GetProductSaleHistoryQuery CreateQuery(
		string? storageCode = null,
		Guid? organizationId = null,
		Guid? preferredOrganizationId = null,
		int? currencyId = null,
		IReadOnlyCollection<string>? sortBy = null,
		bool showCrosses = false,
		Pagination? pagination = null)
	{
		return new GetProductSaleHistoryQuery(
			SaleContext.Product.Id,
			pagination ?? new Pagination(0, 20),
			storageCode,
			organizationId,
			preferredOrganizationId,
			currencyId,
			sortBy ?? [],
			showCrosses);
	}
}
