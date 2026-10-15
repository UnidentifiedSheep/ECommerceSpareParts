using Exceptions;
using FluentAssertions;
using Main.Entities;
using Main.Entities.Product;

namespace Tests.Domain.Product;

public class CharacteristicTests
{
	[Theory]
	[InlineData("  Brake Color  ", "Brake Color", "brake-color")]
	[InlineData("  Размер детали  ", "Размер детали", "razmer-detali")]
	public void Create_TrimsAndNormalizesName(string input, string name, string normalizedName)
	{
		var characteristic = Characteristic.Create(input);

		characteristic.Name.Should().Be(name);
		characteristic.NormalizedName.Should().Be(normalizedName);
	}

	[Fact]
	public void SetName_UpdatesNameAndNormalizedName()
	{
		var characteristic = Characteristic.Create("Color");

		characteristic.SetName("  Engine Volume  ");

		characteristic.Name.Should().Be("Engine Volume");
		characteristic.NormalizedName.Should().Be("engine-volume");
	}

	[Theory]
	[InlineData(null, CharacteristicNameRequiredMessage.Key)]
	[InlineData("   ", CharacteristicNameRequiredMessage.Key)]
	[InlineData("!!!", CharacteristicNormalizedNameRequiredMessage.Key)]
	public void Create_InvalidName_ThrowsLocalizedError(string? name, string expectedKey)
	{
		var act = () => Characteristic.Create(name!);

		act.Should().Throw<InvalidInputException>()
			.Which.LocalizableMessage.MessageKey.Should().Be(expectedKey);
	}

	[Fact]
	public void Create_NameLengthBoundary()
	{
		Characteristic.Create(new string('a', 128)).Name.Should().HaveLength(128);
		var act = () => Characteristic.Create(new string('a', 129));

		act.Should().Throw<InvalidInputException>()
			.Which.LocalizableMessage.MessageKey.Should().Be(ArticleCharacteristicNameMaxLengthMessage.Key);
	}

	[Fact]
	public void SetName_InvalidName_PreservesExistingValues()
	{
		var characteristic = Characteristic.Create("Color");
		var act = () => characteristic.SetName("!!!");

		act.Should().Throw<InvalidInputException>();
		characteristic.Name.Should().Be("Color");
		characteristic.NormalizedName.Should().Be("color");
	}
}
