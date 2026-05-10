using FluentAssertions;
using Main.Entities.Product.ValueObjects;

namespace Tests.Domain.Product;

public class StockTests
{
    [Fact]
    public void Create_Valid()
    {
        var stock = new Stock(10);

        stock.Value.Should().Be(10);
    }

    [Fact]
    public void Negative_Throws()
    {
        var act = () => new Stock(-1);

        act.Should().Throw<InvalidOperationException>();
    }
}