using Exceptions;
using FluentAssertions;
using Main.Entities.Product;

namespace Tests.Domain.Product;

public class ProductImageTests
{
    [Theory]
    [InlineData(1, "image.png", "desc")]
    [InlineData(1, "image.jpg", null)]
    [InlineData(1, "image.webp", "   ")]
    public void Create_ValidData_Succeeds(int productId, string path, string? description)
    {
        var act = () => ProductImage.Create(productId, path, description);

        var model = act.Should().NotThrow().Subject;

        Validate(model, productId, path, description);
    }

    [Theory]
    [InlineData("image.exe")]
    [InlineData("image.txt")]
    [InlineData("image")]
    public void Create_InvalidExtension_Throws(string path)
    {
        var act = () => ProductImage.Create(1, path, "desc");

        act.Should().Throw<InvalidInputException>();
    }

    [Theory]
    [InlineData("image.png")]
    [InlineData("IMAGE.JPG")]
    [InlineData("  image.webp  ")]
    public void SetPath_Valid_Succeeds(string path)
    {
        var model = ProductImage.Create(1, "image.png", null);

        var act = () => model.SetPath(path);

        act.Should().NotThrow();

        model.Path.Should().Be(path.Trim());
    }

    [Theory]
    [InlineData("image.exe")]
    [InlineData("")]
    [InlineData("   ")]
    public void SetPath_Invalid_Throws(string path)
    {
        var model = ProductImage.Create(1, "image.png", null);

        var act = () => model.SetPath(path);

        act.Should().Throw<InvalidInputException>();
    }

    [Theory]
    [InlineData(" desc ")]
    [InlineData(null)]
    [InlineData("   ")]
    public void SetDescription_Normalization_Works(string? description)
    {
        var model = ProductImage.Create(1, "image.png", "old");

        model.SetDescription(description);

        if (string.IsNullOrWhiteSpace(description))
            model.Description.Should().BeNull();
        else
            model.Description.Should().Be(description.Trim());
    }

    private static void Validate(
        ProductImage model,
        int productId,
        string path,
        string? description)
    {
        model.ProductId.Should().Be(productId);
        model.Path.Should().Be(path.Trim());

        if (string.IsNullOrWhiteSpace(description))
            model.Description.Should().BeNull();
        else
            model.Description.Should().Be(description.Trim());
    }
}