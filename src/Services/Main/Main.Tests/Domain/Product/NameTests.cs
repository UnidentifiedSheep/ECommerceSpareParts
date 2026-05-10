using Exceptions;
using FluentAssertions;
using Main.Entities.Product.ValueObjects;

namespace Tests.Domain.Product;

public class NameTests
{
    [Fact]
    public void Create_Capitalizes()
    {
        var name = new Name("product");

        name.Value.Should().Be("Product");
    }

    [Fact]
    public void Invalid_Empty_Throws()
    {
        var act = () => new Name(" ");

        act.Should().Throw<InvalidInputException>();
    }
}