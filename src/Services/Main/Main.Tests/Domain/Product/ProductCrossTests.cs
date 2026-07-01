using Exceptions;
using FluentAssertions;
using Main.Entities.Product;

namespace Tests.Domain.Product;

public class ProductCrossTests
{
    [Theory]
    [InlineData(1, 2)]
    [InlineData(10, 5)]
    [InlineData(100, 101)]
    public void Create_ValidDifferentIds_Succeeds(int left, int right)
    {
        var act = () => ProductCross.Create(left, right);

        var model = act.Should().NotThrow().Subject;

        Validate(
            model,
            left,
            right);
    }

    [Fact]
    public void Create_SameIds_Throws()
    {
        var act = () => ProductCross.Create(1, 1);

        act.Should().Throw<InvalidInputException>();
    }

    [Theory]
    [InlineData(
        1,
        2,
        1,
        2)]
    [InlineData(
        10,
        5,
        5,
        10)]
    [InlineData(
        100,
        101,
        100,
        101)]
    public void Create_NormalizesOrder(
        int left,
        int right,
        int expectedLeft,
        int expectedRight)
    {
        var model = ProductCross.Create(left, right);

        model.LeftProductId.Should().Be(expectedLeft);
        model.RightProductId.Should().Be(expectedRight);
    }

    private static void Validate(
        ProductCross model,
        int left,
        int right)
    {
        model.Should().NotBeNull();

        var expectedLeft = Math.Min(left, right);
        var expectedRight = Math.Max(left, right);

        model.LeftProductId.Should().Be(expectedLeft);
        model.RightProductId.Should().Be(expectedRight);

        model.GetId().Should().Be((expectedLeft, expectedRight));
    }
}