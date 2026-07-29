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

    private GetProductSaleHistoryQuery CreateQuery(
        string? storageName = null,
        Guid? organizationId = null,
        int? currencyId = null,
        string? sortBy = null)
    {
        return new GetProductSaleHistoryQuery(
            SaleContext.Product.Id,
            new Pagination(0, 20),
            storageName,
            organizationId,
            currencyId,
            sortBy);
    }
}
