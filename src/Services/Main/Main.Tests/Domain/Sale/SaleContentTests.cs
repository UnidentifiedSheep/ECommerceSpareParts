using Exceptions;
using FluentAssertions;
using Main.Entities.Sale;

namespace Tests.Domain.Sale;

public class SaleContentTests
{
    [Fact]
    public void Create_ValidData_CalculatesValues()
    {
        var details = new[]
        {
            CreateDetail(count: 2),
            CreateDetail(2, 3)
        };

        var content = SaleContent.Create(
            10,
            100m,
            80m,
            details);

        content.ProductId.Should().Be(10);
        content.Count.Should().Be(5);
        content.Price.Should().Be(80m);
        content.TotalSum.Should().Be(400m);
        content.Discount.Should().Be(0.2m);
        content.Details.Should().BeEquivalentTo(details);
    }

    [Fact]
    public void Create_WithoutDetails_Throws()
    {
        var act = () => SaleContent.Create(
            1,
            100m,
            80m,
            []);

        act.Should().Throw<InvalidInputException>();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(10.001, 1)]
    [InlineData(10, 0)]
    [InlineData(10, -1)]
    [InlineData(10, 1.001)]
    public void Create_WithInvalidPrices_Throws(decimal priceWithoutDiscount, decimal priceWithDiscount)
    {
        var act = () => SaleContent.Create(
            1,
            priceWithoutDiscount,
            priceWithDiscount,
            [CreateDetail()]);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void Create_WhenDiscountedPriceGreaterThanPrice_Throws()
    {
        var act = () => SaleContent.Create(
            1,
            100m,
            101m,
            [CreateDetail()]);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetPriceAndDetails_ReplacesDetailsAndRecalculates()
    {
        var content = Create();
        var details = new[]
        {
            CreateDetail(2, 1),
            CreateDetail(3, 4)
        };

        content.SetPriceAndDetails(
            50m,
            40m,
            details);

        content.Count.Should().Be(5);
        content.Price.Should().Be(40m);
        content.TotalSum.Should().Be(200m);
        content.Discount.Should().Be(0.2m);
        content.Details.Should().BeEquivalentTo(details);
    }

    [Theory]
    [InlineData("   ", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    [InlineData(" test ", "test")]
    public void SetComment_HandlesCases(string? input, string? expected)
    {
        var content = Create();

        content.SetComment(input);

        content.Comment.Should().Be(expected);
    }

    [Fact]
    public void SetComment_WhenTooLong_Throws()
    {
        var content = Create();

        var act = () => content.SetComment(new string('a', 257));

        act.Should().Throw<InvalidInputException>();
    }

    private static SaleContent Create()
    {
        return SaleContent.Create(
            1,
            100m,
            80m,
            [CreateDetail(count: 2)]);
    }

    private static SaleContentDetail CreateDetail(
        int storageContentId = 1,
        int count = 1)
    {
        return SaleContentDetail.Create(
            storageContentId,
            1,
            10m,
            count,
            DateTime.UtcNow);
    }
}