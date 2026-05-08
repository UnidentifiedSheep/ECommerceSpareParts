using Exceptions;
using FluentAssertions;
using Main.Entities.Product;

namespace Main.Tests.Domain.Product;

public class ProductCharacteristicTests
{
    [Theory]
    [InlineData(1, "Color", "Red")]
    [InlineData(2, "Size", "Large")]
    [InlineData(3, "Weight", "100g")]
    public void Create_ValidData_Succeeds(int productId, string name, string value)
    {
        var act = () => ProductCharacteristic.Create(productId, name, value);

        var model = act.Should().NotThrow().Subject;

        Validate(model, productId, name.Trim(), value.Trim());
    }

    [Fact]
    public void Create_EmptyName_Throws_InvalidOperationException()
    {
        var act = () => ProductCharacteristic.Create(1, "   ", "value");

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    public void Create_ValueTooShort_Throws(string value)
    {
        var act = () => ProductCharacteristic.Create(1, "Name", value);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void Create_ValueTooLong_Throws()
    {
        var longValue = new string('x', 200);

        var act = () => ProductCharacteristic.Create(1, "Name", longValue);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetName_Valid_Succeeds()
    {
        var model = ProductCharacteristic.Create(1, "Name", "Value");

        var act = () => model.SetName("NewName");

        act.Should().NotThrow();

        model.Name.Should().Be("NewName");
    }

    [Fact]
    public void SetName_Empty_Throws()
    {
        var model = ProductCharacteristic.Create(1, "Name", "Value");

        var act = () => model.SetName("   ");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SetValue_Valid_Succeeds()
    {
        var model = ProductCharacteristic.Create(1, "Name", "Value");

        var act = () => model.SetValue("NewValue");

        act.Should().NotThrow();

        model.Value.Should().Be("NewValue");
    }

    [Theory]
    [InlineData("ab")]
    public void SetValue_TooShort_Throws(string value)
    {
        var model = ProductCharacteristic.Create(1, "Name", "Value");

        var act = () => model.SetValue(value);

        act.Should().Throw<InvalidInputException>();
    }

    private static void Validate(
        ProductCharacteristic model,
        int productId,
        string name,
        string value)
    {
        model.Should().NotBeNull();

        model.ProductId.Should().Be(productId);
        model.Name.Should().Be(name);
        model.Value.Should().Be(value);

        model.GetId().Should().Be((productId, name));
    }
}