using Analytics.Entities;
using FluentAssertions;

namespace Analytics.Integration.Tests.Domain;

public class SalesFactTests
{
    [Fact]
    public void Create_CalculatesDailyChartValuesInBaseCurrency()
    {
        var organizationId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();

        var fact = SalesFact.Create(
            Guid.NewGuid(),
            2,
            1,
            organizationId,
            buyerId,
            DateTime.UtcNow,
            DateTime.UtcNow,
            [
                CreateContent(
                    id: 1,
                    price: 100m,
                    priceInBaseCurrency: 120m,
                    count: 2,
                    details:
                    [
                        CreateDetail(1, 1, 70m, 1),
                        CreateDetail(2, 1, 80m, 1)
                    ]),
                CreateContent(
                    id: 2,
                    price: 40m,
                    priceInBaseCurrency: 50m,
                    count: 3,
                    details: [CreateDetail(3, 2, 20m, 3)])
            ]);

        fact.OrganizationId.Should().Be(organizationId);
        fact.BuyerId.Should().Be(buyerId);
        fact.RevenueInBaseCurrency.Should().Be(390m);
        fact.CostInBaseCurrency.Should().Be(210m);
        fact.GrossProfitInBaseCurrency.Should().Be(180m);
        fact.ProductsCount.Should().Be(5);
    }

    [Fact]
    public void Update_RecalculatesDailyChartValuesAndDimensions()
    {
        var fact = SalesFact.Create(
            Guid.NewGuid(),
            1,
            1,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            [CreateContent(1, 100m, 100m, 1, [CreateDetail(1, 1, 60m, 1)])]);
        var organizationId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();

        fact.Update(
            2,
            1,
            organizationId,
            buyerId,
            DateTime.UtcNow,
            DateTime.UtcNow,
            [CreateContent(2, 200m, 250m, 2, [CreateDetail(2, 2, 75m, 2)])]);

        fact.OrganizationId.Should().Be(organizationId);
        fact.BuyerId.Should().Be(buyerId);
        fact.RevenueInBaseCurrency.Should().Be(500m);
        fact.CostInBaseCurrency.Should().Be(150m);
        fact.GrossProfitInBaseCurrency.Should().Be(350m);
        fact.ProductsCount.Should().Be(2);
    }

    [Fact]
    public void CreateDeleted_CreatesTombstone()
    {
        var id = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        var fact = SalesFact.CreateDeleted(id, occurredAt);

        fact.Id.Should().Be(id);
        fact.IsDeleted.Should().BeTrue();
        fact.ProcessedAt.Should().Be(occurredAt);
        fact.SaleContents.Should().BeEmpty();
    }

    [Fact]
    public void Update_DeletedFact_RestoresFactWithRecalculatedValues()
    {
        var fact = SalesFact.CreateDeleted(
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(-1));

        fact.Update(
            2,
            1,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            [CreateContent(1, 100m, 120m, 2, [CreateDetail(1, 1, 70m, 2)])]);

        fact.IsDeleted.Should().BeFalse();
        fact.RevenueInBaseCurrency.Should().Be(240m);
        fact.CostInBaseCurrency.Should().Be(140m);
        fact.GrossProfitInBaseCurrency.Should().Be(100m);
        fact.ProductsCount.Should().Be(2);
    }

    private static SaleContent CreateContent(
        int id,
        decimal price,
        decimal priceInBaseCurrency,
        int count,
        IEnumerable<SaleContentDetail> details)
    {
        return SaleContent.Create(
            id,
            Guid.NewGuid(),
            id,
            price,
            priceInBaseCurrency,
            count,
            0m,
            details);
    }

    private static SaleContentDetail CreateDetail(
        int id,
        int saleContentId,
        decimal buyPriceInBaseCurrency,
        int count)
    {
        return SaleContentDetail.Create(
            id,
            saleContentId,
            1,
            buyPriceInBaseCurrency,
            buyPriceInBaseCurrency,
            count,
            DateTime.UtcNow);
    }
}
