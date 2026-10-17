using System.Text.RegularExpressions;
using Exceptions;
using FluentAssertions;
using Main.Entities.Product;

namespace Tests.Domain.Product;

public class ProductImageTests
{
	[Theory]
	[InlineData(".png", ".png")]
	[InlineData("jpg", ".jpg")]
	[InlineData(" .JPEG ", ".jpeg")]
	[InlineData(" WEBP ", ".webp")]
	public void Create_ValidExtension_GeneratesPath(string extension, string expectedExtension)
	{
		var image = ProductImage.Create(42, extension);

		image.ProductId.Should().Be(42);
		image
			.StorageKey
			.Should()
			.MatchRegex($@"^products/42_[0-9a-f]{{32}}{Regex.Escape(expectedExtension)}$");
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
		var act = () => ProductImage.Create(1, extension);

		act
			.Should()
			.Throw<InvalidInputException>()
			.Which
			.LocalizableMessage
			.MessageKey
			.Should()
			.Be("article.image.invalid.extension");
	}

	[Fact]
	public void Create_CalledTwice_GeneratesUniquePaths()
	{
		var first = ProductImage.Create(1, ".png");
		var second = ProductImage.Create(1, ".png");

		second.StorageKey.Should().NotBe(first.StorageKey);
	}

	[Theory]
	[InlineData("photo.JPG")]
	[InlineData("supplier/Фото товара.webp")]
	public void CreateFromStorageKey_PreservesTheUploadedObjectKey(string storageKey)
	{
		var image = ProductImage.CreateFromStorageKey(42, storageKey);

		image.ProductId.Should().Be(42);
		image.StorageKey.Should().Be($"products/{storageKey}");
		image.GetId().Should().Be((42, $"products/{storageKey}"));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("/photo.jpg")]
	[InlineData("../photo.jpg")]
	[InlineData("./photo.jpg")]
	[InlineData("folder/")]
	[InlineData("folder//photo.jpg")]
	[InlineData("folder\\photo.jpg")]
	[InlineData("photo\n.jpg")]
	public void CreateFromStorageKey_InvalidPath_ThrowsExpectedError(string? storageKey)
	{
		var act = () => ProductImage.CreateFromStorageKey(42, storageKey!);

		act.Should().Throw<InvalidInputException>()
			.Which.LocalizableMessage.MessageKey.Should().Be("article.image.invalid.storage.key");
	}

	[Fact]
	public void CreateFromStorageKey_EnforcesTheDatabaseKeyLength()
	{
		var longestKey = $"{new string('a', 242)}.jpg";
		ProductImage.CreateFromStorageKey(42, longestKey).StorageKey.Should().HaveLength(255);

		var act = () => ProductImage.CreateFromStorageKey(42, longestKey + "x");

		act.Should().Throw<InvalidInputException>()
			.Which.LocalizableMessage.MessageKey.Should().Be("article.image.invalid.storage.key");
	}

	[Theory]
	[InlineData("photo.exe")]
	[InlineData("photo")]
	[InlineData("photo.jpg?download=1")]
	public void CreateFromStorageKey_UnsupportedExtension_ThrowsExpectedError(string storageKey)
	{
		var act = () => ProductImage.CreateFromStorageKey(42, storageKey);

		act.Should().Throw<InvalidInputException>()
			.Which.LocalizableMessage.MessageKey.Should().Be("article.image.invalid.extension");
	}
}
