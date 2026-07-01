using Enums;
using Exceptions;
using FluentAssertions;
using Main.Entities.Product;

namespace Tests.Domain.Product;

public class ProductSizeTests
{
    [Theory]
    [InlineData(
        10.1,
        5.2,
        3.3,
        DimensionUnit.Centimeter)]
    [InlineData(
        1,
        1,
        1,
        DimensionUnit.Meter)]
    [InlineData(
        0.99,
        0.99,
        0.99,
        DimensionUnit.Millimeter)]
    public void CreateSize_ValidData_Succeeds(
        decimal length,
        decimal width,
        decimal height,
        DimensionUnit unit)
    {
        var act = () => ProductSize.Create(
            1,
            length,
            width,
            height,
            unit);
        var model = act.Should().NotThrow().Subject;

        ValidateModel(
            model,
            1,
            length,
            width,
            height,
            unit);
        model.VolumeM3.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(
        -1,
        1,
        1)]
    [InlineData(
        1,
        -1,
        1)]
    [InlineData(
        1,
        1,
        -1)]
    [InlineData(
        0,
        1,
        1)]
    [InlineData(
        1000.324,
        1,
        1)]
    public void CreateSize_InvalidLengthWidthHeight_Throws(
        decimal length,
        decimal width,
        decimal height)
    {
        var act = () => ProductSize.Create(
            1,
            length,
            width,
            height,
            DimensionUnit.Centimeter);
        act.Should().Throw<InvalidInputException>();
    }

    [Theory]
    [InlineData(10.1)]
    [InlineData(0.1)]
    [InlineData(0.99)]
    public void SetLength_ValidData_Succeeds(decimal length)
    {
        var model = ProductSize.Create(
            1,
            1,
            1,
            1,
            DimensionUnit.Meter);

        var act = () => model.SetLength(length);

        act.Should().NotThrow();

        model.Length.Should().Be(length);
        model.VolumeM3.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1000.324)]
    public void SetLength_InvalidData_Throws(decimal length)
    {
        var model = ProductSize.Create(
            1,
            1,
            1,
            1,
            DimensionUnit.Meter);

        var act = () => model.SetLength(length);

        act.Should().Throw<InvalidInputException>();

        ValidateModel(
            model,
            1,
            1,
            1,
            1,
            DimensionUnit.Meter);
    }

    [Theory]
    [InlineData(10.1)]
    [InlineData(0.1)]
    [InlineData(0.99)]
    public void SetWidth_ValidData_Succeeds(decimal width)
    {
        var model = ProductSize.Create(
            1,
            1,
            1,
            1,
            DimensionUnit.Meter);

        var act = () => model.SetWidth(width);

        act.Should().NotThrow();

        model.Width.Should().Be(width);
        model.VolumeM3.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1000.324)]
    public void SetWidth_InvalidData_Throws(decimal width)
    {
        var model = ProductSize.Create(
            1,
            1,
            1,
            1,
            DimensionUnit.Meter);

        var act = () => model.SetWidth(width);

        act.Should().Throw<InvalidInputException>();

        ValidateModel(
            model,
            1,
            1,
            1,
            1,
            DimensionUnit.Meter);
    }

    [Theory]
    [InlineData(10.1)]
    [InlineData(0.1)]
    [InlineData(0.99)]
    public void SetHeight_ValidData_Succeeds(decimal height)
    {
        var model = ProductSize.Create(
            1,
            1,
            1,
            1,
            DimensionUnit.Meter);

        var act = () => model.SetHeight(height);

        act.Should().NotThrow();

        model.Height.Should().Be(height);
        model.VolumeM3.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1000.324)]
    public void SetHeight_InvalidData_Throws(decimal height)
    {
        var model = ProductSize.Create(
            1,
            1,
            1,
            1,
            DimensionUnit.Meter);

        var act = () => model.SetHeight(height);

        act.Should().Throw<InvalidInputException>();

        ValidateModel(
            model,
            1,
            1,
            1,
            1,
            DimensionUnit.Meter);
    }

    [Theory]
    [InlineData(DimensionUnit.Meter)]
    [InlineData(DimensionUnit.Centimeter)]
    [InlineData(DimensionUnit.Millimeter)]
    public void SetUnit_ValidData_Succeeds(DimensionUnit unit)
    {
        var model = ProductSize.Create(
            1,
            1,
            1,
            1,
            DimensionUnit.Meter);

        var act = () => model.SetUnit(unit);

        act.Should().NotThrow();

        model.Unit.Should().Be(unit);
        model.VolumeM3.Should().BeGreaterThan(0);
    }

    private static void ValidateModel(
        ProductSize model,
        int productId,
        decimal length,
        decimal width,
        decimal height,
        DimensionUnit unit)
    {
        model.Should().NotBeNull();
        model.ProductId.Should().Be(productId);
        model.Length.Should().Be(length);
        model.Width.Should().Be(width);
        model.Height.Should().Be(height);
        model.Unit.Should().Be(unit);
    }
}