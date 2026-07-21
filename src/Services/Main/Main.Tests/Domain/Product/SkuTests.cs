using Exceptions;
using FluentAssertions;
using Main.Entities.Product.ValueObjects;

namespace Tests.Domain.Product;

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

    [Fact]
    public void IsValid_ValidSku_ReturnsTrueWithoutException()
    {
        var result = Sku.IsValid(" ABC-123 ", out var exception);

        result.Should().BeTrue();
        exception.Should().BeNull();
    }

    [Theory]
    [InlineData(null, "article.articleNumber.must.not.be.empty")]
    [InlineData("   ", "article.articleNumber.must.not.be.empty")]
    [InlineData("AB", "article.articleNumber.min.length.3")]
    public void IsValid_InvalidSku_ReturnsFalseWithException(
        string? value,
        string expectedErrorKey)
    {
        var result = Sku.IsValid(value, out var exception);

        result.Should().BeFalse();
        exception.Should().BeOfType<InvalidInputException>();
        ((InvalidInputException)exception!).MessageKey.Should().Be(expectedErrorKey);
    }

    [Fact]
    public void IsValid_WhenSkuExceedsMaximumLength_ReturnsFalseWithException()
    {
        var result = Sku.IsValid(new string('A', 129), out var exception);

        result.Should().BeFalse();
        exception.Should().BeOfType<InvalidInputException>();
        ((InvalidInputException)exception!).MessageKey
            .Should()
            .Be("article.articleNumber.max.length.128");
    }
}
