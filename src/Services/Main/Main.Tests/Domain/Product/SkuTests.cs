using Exceptions;
using FluentAssertions;
using Main.Entities.Product.ValueObjects;

namespace Main.Tests.Domain.Product;

public class SkuTests
{
    [Fact]
    public void Create_Normalizes()
    {
        var sku = new Sku("ABC-123");

        sku.Value.Should().Be("ABC-123");
        sku.NormalizedValue.Should().Be("ABC123");
    }

    [Fact]
    public void Invalid_Empty_Throws()
    {
        var act = () => new Sku("   ");

        act.Should().Throw<InvalidInputException>();
    }
}