using Exceptions;
using FluentAssertions;
using Main.Entities.Product.ValueObjects;

namespace Tests.Domain.Product;

public class IndicatorTests
{
    [Fact]
    public void Create_Trims()
    {
        var ind = new Indicator("  test  ");

        ind.Value.Should().Be("test");
    }

    [Fact]
    public void TooLong_Throws()
    {
        var longValue = new string('x', 100);

        var act = () => new Indicator(longValue);

        act.Should().Throw<InvalidInputException>();
    }
}