using Exceptions;
using FluentAssertions;
using Main.Entities.Product;

namespace Tests.Domain.Product;

public class ProductContentTests
{
    [Theory]
    [InlineData(1, 2, 5)]
    [InlineData(10, 20, 0)]
    [InlineData(100, 101, 999)]
    public void Create_ValidData_Succeeds(int parentId, int childId, int quantity)
    {
        var act = () => ProductContent.Create(parentId, childId, quantity);

        var model = act.Should().NotThrow().Subject;

        Validate(model, parentId, childId, quantity);
    }

    [Fact]
    public void Create_SameProductIds_Throws()
    {
        var act = () => ProductContent.Create(1, 1, 5);

        act.Should().Throw<InvalidInputException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(100)]
    public void SetQuantity_Valid_Succeeds(int quantity)
    {
        var model = ProductContent.Create(1, 2, 1);

        var act = () => model.SetQuantity(quantity);

        act.Should().NotThrow();

        model.Quantity.Should().Be(quantity);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void SetQuantity_Negative_Throws(int quantity)
    {
        var model = ProductContent.Create(1, 2, 1);

        var act = () => model.SetQuantity(quantity);

        act.Should().Throw<InvalidInputException>();

        model.Quantity.Should().Be(1);
    }

    private static void Validate(
        ProductContent model,
        int parentId,
        int childId,
        int quantity)
    {
        model.Should().NotBeNull();

        model.ParentProductId.Should().Be(parentId);
        model.ChildProductId.Should().Be(childId);
        model.Quantity.Should().Be(quantity);

        model.GetId().Should().Be((parentId, childId));
    }
}