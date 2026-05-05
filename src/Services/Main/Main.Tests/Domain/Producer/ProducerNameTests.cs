using Exceptions;
using FluentAssertions;
using Main.Entities.Producer.ValueObjects;

namespace Main.Tests.Domain.Producer;

public class ProducerNameTests
{
    [Fact]
    public void Create_CapitalizesFirstLetter()
    {
        var name = new Name("sony");

        name.Value.Should().Be("Sony");
    }

    [Fact]
    public void Create_TrimsInput()
    {
        var name = new Name("   sony   ");

        name.Value.Should().Be("Sony");
    }

    [Fact]
    public void TooShort_Throws()
    {
        var act = () => new Name("ab");

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void Empty_Throws()
    {
        var act = () => new Name("   ");

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void TooLong_Throws()
    {
        var longName = new string('a', 300);

        var act = () => new Name(longName);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void ImplicitConversion_ToString_Works()
    {
        Name name = "sony";

        string value = name;

        value.Should().Be("Sony");
    }

    [Fact]
    public void ImplicitConversion_FromString_Works()
    {
        Name name = "sony";

        name.Value.Should().Be("Sony");
    }
}