using FluentAssertions;
using Main.Entities.Sale;

namespace Tests.Domain.Sale;

public class SaleContentDetailTests
{
    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var purchaseDate = DateTime.UtcNow;

        var detail = SaleContentDetail.Create(
            1,
            2,
            10m,
            3,
            purchaseDate);

        detail.StorageContentId.Should().Be(1);
        detail.CurrencyId.Should().Be(2);
        detail.BuyPrice.Should().Be(10m);
        detail.Count.Should().Be(3);
        detail.PurchaseDatetime.Should().Be(purchaseDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidBuyPrice_Throws(decimal buyPrice)
    {
        var act = () => SaleContentDetail.Create(
            1,
            1,
            buyPrice,
            1,
            DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidCount_Throws(int count)
    {
        var act = () => SaleContentDetail.Create(
            1,
            1,
            10m,
            count,
            DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }
}