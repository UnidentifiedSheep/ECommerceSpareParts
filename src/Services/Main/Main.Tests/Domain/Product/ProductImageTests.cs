using System.Text.RegularExpressions;
using Exceptions;
using FluentAssertions;
using Main.Entities.Product;

namespace Tests.Domain.Product;

public partial class ProductImageTests
{
    [Theory]
    [InlineData(".png", ".png")]
    [InlineData("jpg", ".jpg")]
    [InlineData(" .JPEG ", ".jpeg")]
    [InlineData(" WEBP ", ".webp")]
    public void Create_ValidExtension_GeneratesPath(
        string extension,
        string expectedExtension)
    {
        var image = ProductImage.Create(
            42,
            extension);

        image.ProductId.Should().Be(42);
        image.StorageKey.Should().MatchRegex(
            $@"^products/42_[0-9a-f]{{32}}{Regex.Escape(expectedExtension)}$");
    }

    [Theory]
    [InlineData("image.png")]
    [InlineData(".exe")]
    [InlineData("txt")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(".")]
    public void Create_InvalidExtension_ThrowsExpectedError(string extension)
    {
        var act = () => ProductImage.Create(
            1,
            extension);

        act.Should()
            .Throw<InvalidInputException>()
            .Which.MessageKey.Should()
            .Be("article.image.invalid.extension");
    }

    [Fact]
    public void Create_CalledTwice_GeneratesUniquePaths()
    {
        var first = ProductImage.Create(1, ".png");
        var second = ProductImage.Create(1, ".png");

        second.StorageKey.Should().NotBe(first.StorageKey);
    }
}
