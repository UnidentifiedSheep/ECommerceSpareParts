using Abstractions.Models;
using FluentAssertions;
using Main.Application.Handlers.Sales.GetProductSaleHistory;
using Tests.DataBuilders.Sale;
using Tests.TestContainers.Combined;
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

        var result = await Mediator.Send(CreateQuery());

        var history = result.History.Should().ContainSingle().Subject;
        history.SaleContentId.Should().Be(content.Id);
        history.ProductId.Should().Be(content.ProductId);
        history.OrganizationId.Should().Be(sale.OrganizationId);
        history.CurrencyId.Should().Be(sale.CurrencyId);
        history.StorageName.Should().Be(sale.StorageName);
        history.Quantity.Should().Be(content.Count);
        history.Discount.Should().Be(content.Discount);
        history.Price.Should().Be(content.Price);
        history.AverageBuyPrice.Should().Be(expectedAverageBuyPrice);
        history.SaleDate.Should().BeCloseTo(
            sale.SaleDatetime.ToUniversalTime(),
            TimeSpan.FromMicroseconds(1));
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
            .WithStorageName(completedSale.StorageName)
            .WithTransactionId(completedSale.TransactionId)
            .Build();

        await Context.AddAsync(draftSale);
        await Context.SaveChangesAsync();

        var result = await Mediator.Send(CreateQuery());

        result.History.Should().ContainSingle()
            .Which.SaleContentId.Should().Be(completedContent.Id);
    }

    [Fact]
    public async Task GetProductSaleHistory_AppliesOptionalFiltersAndSort()
    {
        var sale = SaleContext.Sale;

        var result = await Mediator.Send(
            CreateQuery(
                storageName: sale.StorageName,
                organizationId: sale.OrganizationId,
                preferredOrganizationId: sale.OrganizationId,
                currencyId: sale.CurrencyId,
                sortBy: "averageBuyPrice_desc"));

        result.History.Should().ContainSingle();
    }

    [Fact]
    public async Task GetProductSaleHistory_WhenFilterDoesNotMatch_ReturnsEmptyHistory()
    {
        var result = await Mediator.Send(
            CreateQuery(organizationId: Guid.NewGuid()));

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
            .WithStorageName(preferredSale.StorageName)
            .WithTransactionId(preferredSale.TransactionId)
            .WithSaleDate(preferredSale.SaleDatetime.AddDays(1))
            .Completed()
            .Build();

        await Context.AddAsync(fallbackSale);
        await Context.SaveChangesAsync();

        var result = await Mediator.Send(
            CreateQuery(
                preferredOrganizationId: preferredSale.OrganizationId,
                sortBy: "saleDate_desc"));

        result.History.Select(x => x.OrganizationId).Should().Equal(
            preferredSale.OrganizationId,
            fallbackOrganizationId);
    }

    private GetProductSaleHistoryQuery CreateQuery(
        string? storageName = null,
        Guid? organizationId = null,
        Guid? preferredOrganizationId = null,
        int? currencyId = null,
        string? sortBy = null)
    {
        return new GetProductSaleHistoryQuery(
            SaleContext.Product.Id,
            new Pagination(0, 20),
            storageName,
            organizationId,
            preferredOrganizationId,
            currencyId,
            sortBy);
    }
}
