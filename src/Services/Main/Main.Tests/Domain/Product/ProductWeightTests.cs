using Enums;
using Exceptions;
using FluentAssertions;
using Main.Entities.Product;

namespace Tests.Domain.Product;

public class ProductWeightTests
{
    [Theory]
    [InlineData(10.1, WeightUnit.Tonne)]
    [InlineData(0.1, WeightUnit.Gram)]
    [InlineData(0.99, WeightUnit.Kilogram)]
    public void CreateWeight_ValidData_Succeeds(decimal weight, WeightUnit unit)
    {
        var act = () => ProductWeight.Create(1, weight, unit);
        var model = act.Should().NotThrow().Subject;

        ValidateModel(model, 1, weight, unit);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-1000.2222)]
    [InlineData(0)]
    [InlineData(1000.324)]
    public void CreateWeight_InvalidWeight_Throws(decimal weight)
    {
        var act = () => ProductWeight.Create(1, weight, WeightUnit.Kilogram);
        act.Should().Throw<InvalidInputException>();
    }

    [Theory]
    [InlineData(10.1, WeightUnit.Tonne)]
    [InlineData(0.1, WeightUnit.Gram)]
    [InlineData(0.99, WeightUnit.Kilogram)]
    public void UpdateWeight_ValidData_Succeeds(decimal weight, WeightUnit unit)
    {
        var model = ProductWeight.Create(1, 1, WeightUnit.Kilogram);

        var act = () => model.Update(weight, unit);

        act.Should().NotThrow();

        ValidateModel(model, 1, weight, unit);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-1000.2222)]
    [InlineData(0)]
    [InlineData(1000.324)]
    public void UpdateWeight_InvalidWeight_Throws(decimal weight)
    {
        var model = ProductWeight.Create(1, 1, WeightUnit.Kilogram);

        var act = () => model.Update(weight, WeightUnit.Kilogram);
        act.Should().Throw<InvalidInputException>();

        ValidateModel(model, 1, 1, WeightUnit.Kilogram);
    }

    private static void ValidateModel(
        ProductWeight model,
        int productId,
        decimal weight,
        WeightUnit unit)
    {
        model.Should().NotBeNull();
        model.ProductId.Should().Be(productId);
        model.Weight.Should().Be(weight);
        model.Unit.Should().Be(unit);
    }
}