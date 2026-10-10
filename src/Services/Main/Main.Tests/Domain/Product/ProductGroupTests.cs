using Exceptions;
using FluentAssertions;
using Main.Entities;
using Main.Entities.Product;

namespace Tests.Domain.Product;

public class ProductGroupTests
{
	[Fact]
	public void Create_TrimsNameAndGeneratesSlug()
	{
		var group = ProductGroup.Create("  Brake Pads  ");

		group.Name.Should().Be("Brake Pads");
		group.NormalizedName.Should().Be("brake-pads");
	}

	[Fact]
	public void Create_RussianName_TransliteratesAndNormalizes()
	{
		var group = ProductGroup.Create("  Масло моторное  ");

		group.Name.Should().Be("Масло моторное");
		group.NormalizedName.Should().Be("maslo-motornoe");

		group.SetName("МАСЛО МОТОРНОЕ");
		group.NormalizedName.Should().Be("maslo-motornoe");
	}

	[Fact]
	public void SetName_UpdatesNameAndSlug()
	{
		var group = ProductGroup.Create("Brake Pads");

		group.SetName("  Oil Filters  ");

		group.Name.Should().Be("Oil Filters");
		group.NormalizedName.Should().Be("oil-filters");
	}

	[Theory]
	[InlineData(null, ProductGroupNameRequiredMessage.Key)]
	[InlineData("   ", ProductGroupNameRequiredMessage.Key)]
	[InlineData("ab", ProductGroupNameMinLengthMessage.Key)]
	[InlineData("!!!", ProductGroupNormalizedNameRequiredMessage.Key)]
	public void Create_InvalidName_ThrowsLocalizedError(string? name, string expectedMessageKey)
	{
		var act = () => ProductGroup.Create(name!);

		act.Should().Throw<InvalidInputException>()
			.Which.LocalizableMessage.MessageKey.Should().Be(expectedMessageKey);
	}

	[Fact]
	public void Create_NameAtMaximumLength_Succeeds()
	{
		var name = new string('a', 256);

		var group = ProductGroup.Create(name);

		group.Name.Should().Be(name);
		group.NormalizedName.Should().HaveLength(256);
	}

	[Fact]
	public void Create_NameOverMaximumLength_ThrowsLocalizedError()
	{
		var act = () => ProductGroup.Create(new string('a', 257));

		act.Should().Throw<InvalidInputException>()
			.Which.LocalizableMessage.MessageKey.Should().Be(ProductGroupNameMaxLengthMessage.Key);
	}

	[Fact]
	public void SetName_InvalidName_DoesNotChangeExistingValues()
	{
		var group = ProductGroup.Create("Brake Pads");

		var act = () => group.SetName("!!!");

		act.Should().Throw<InvalidInputException>();
		group.Name.Should().Be("Brake Pads");
		group.NormalizedName.Should().Be("brake-pads");
	}
}
