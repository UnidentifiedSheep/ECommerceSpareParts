using FluentAssertions;
using Main.Entities.Product.ValueObjects;
using ProductDomain = Main.Entities.Product.Product;

namespace Tests.Domain.Product;

public class ProductTests
{
    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var product = ProductDomain.Create(
            new Sku("ABC-123"),
            new Name("test product"),
            1,
            "desc");

        product.Sku.Value.Should().Be("ABC-123");
        product.Name.Value.Should().Be("Test product");
        product.ProducerId.Should().Be(1);
        product.Description.Should().Be("desc");
        product.Stock.Value.Should().Be(0);
    }

    [Fact]
    public void SetDescription_Normalizes()
    {
        var product = Create();

        product.SetDescription("  hello  ");

        product.Description.Should().Be("hello");
    }

    [Fact]
    public void SetDescription_EmptyBecomesNull()
    {
        var product = Create();

        product.SetDescription("   ");

        product.Description.Should().BeNull();
    }

    [Fact]
    public void IncreaseStock_AddsValue()
    {
        var product = Create();

        product.IncreaseStock(10);

        product.Stock.Value.Should().Be(10);
    }

    [Fact]
    public void IncreaseStock_AllowsNegative_CurrentBug()
    {
        var product = Create();

        var act = () => product.IncreaseStock(-5);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SetPackingUnit_Negative_Throws()
    {
        var product = Create();

        var act = () => product.SetPackingUnit(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetPopularity_Negative_Throws()
    {
        var product = Create();

        var act = () => product.SetPopularity(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ValueObjects_WorkCorrectly()
    {
        var sku = new Sku("ABC-123");
        var name = new Name("test product");

        sku.NormalizedValue.Should().Be("ABC123");
        name.Value.Should().Be("Test product");
    }

    private static ProductDomain Create()
    {
        return ProductDomain.Create(
            new Sku("ABC-123"),
            new Name("test product"),
            1,
            "desc");
    }
}