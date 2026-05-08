using FluentAssertions;
using Main.Entities.Purchase;

namespace Main.Tests.Domain.Purchase;

public class PurchaseContentTests
{
    [Fact]
    public void Create_ValidData_CalculatesTotal()
    {
        var item = Create();

        item.TotalSum.Should().Be(20m);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    [InlineData(0, 0)]
    public void Create_ZeroValues_Throws(int count, decimal price)
    {
        var act = () => PurchaseContent.Create(
            1,
            count,
            price,
            null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void SetCount_InvalidValues_Throws(int count)
    {
        var item = Create();

        var act = () => item.SetCount(count);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(5, 50)]
    [InlineData(3, 30)]
    public void SetCount_RecalculatesTotal(int newCount, decimal expected)
    {
        var item = Create();

        item.SetCount(newCount);

        item.TotalSum.Should().Be(expected);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void SetPrice_InvalidValues_Throws(decimal price)
    {
        var item = Create();

        var act = () => item.SetPrice(price);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(3, 6)]
    [InlineData(5, 10)]
    public void SetPrice_RecalculatesTotal(decimal newPrice, decimal expected)
    {
        var item = Create();

        item.SetPrice(newPrice);

        item.TotalSum.Should().Be(expected);
    }

    [Theory]
    [InlineData("   ", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    [InlineData(" test ", "test")]
    public void SetComment_HandlesCases(string? input, string? expected)
    {
        var item = Create();

        item.SetComment(input);

        item.Comment.Should().Be(expected);
    }

    [Fact]
    public void SetLogistic_CreatesOrUpdates()
    {
        var item = Create();

        item.SetLogistic(1, 2, 3);
        item.SetLogistic(2, 3, 4);

        item.PurchaseContentLogistic!.WeightKg.Should().Be(2);
        item.PurchaseContentLogistic.AreaM3.Should().Be(3);
        item.PurchaseContentLogistic.Price.Should().Be(4);
    }

    [Theory]
    [InlineData(3, 5, 15)]
    [InlineData(10, 2, 20)]
    [InlineData(4, 7, 28)]
    public void TotalSum_AfterUpdates_IsCorrect(int count, decimal price, decimal expected)
    {
        var item = Create();

        item.SetCount(count);
        item.SetPrice(price);

        item.TotalSum.Should().Be(expected);
    }

    [Theory]
    [InlineData(3, 5)]
    [InlineData(10, 2)]
    public void TotalSum_OrderIndependent(int count, decimal price)
    {
        var item1 = Create();
        item1.SetCount(count);
        item1.SetPrice(price);

        var item2 = Create();
        item2.SetPrice(price);
        item2.SetCount(count);

        item1.TotalSum.Should().Be(item2.TotalSum);
    }

    [Theory]
    [InlineData(2, 3, 4, 5, 20)]
    [InlineData(1, 2, 3, 4, 12)]
    public void TotalSum_MultipleChanges_IsConsistent(
        decimal p1,
        int c1,
        decimal p2,
        int c2,
        decimal expected)
    {
        var item = Create();

        item.SetPrice(p1);
        item.SetCount(c1);
        item.SetPrice(p2);
        item.SetCount(c2);

        item.TotalSum.Should().Be(expected);
    }

    [Fact]
    public void FullLifecycle_StateIsAlwaysConsistent()
    {
        var item = Create();

        item.SetPrice(2m);
        item.SetCount(3);
        item.SetComment("test");
        item.SetLogistic(1, 2, 3);

        item.TotalSum.Should().Be(6m);
        item.Comment.Should().Be("test");
        item.PurchaseContentLogistic.Should().NotBeNull();
    }

    private static PurchaseContent Create()
    {
        return PurchaseContent.Create(
            1,
            2,
            10m,
            null);
    }
}