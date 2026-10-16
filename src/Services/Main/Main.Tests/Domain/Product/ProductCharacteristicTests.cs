using Exceptions;
using FluentAssertions;
using Main.Entities;
using Main.Entities.Product;

namespace Tests.Domain.Product;

public class ProductCharacteristicTests
{
	[Theory]
	[InlineData(1, 10, "Red")]
	[InlineData(2, 20, "  Large  ")]
	[InlineData(1, 10, " a ")]
	[InlineData(2, 20, " ab ")]
	public void Create_UsesProductAndCharacteristicIds(int productId, int characteristicId, string value)
	{
		var model = ProductCharacteristic.Create(productId, characteristicId, value);

		model.ProductId.Should().Be(productId);
		model.CharacteristicId.Should().Be(characteristicId);
		model.Value.Should().Be(value.Trim());
		model.GetId().Should().Be((productId, characteristicId));
		ProductCharacteristic.GetKeySelector().Compile()(model).Should().Be(model.GetId());
		var matches = ProductCharacteristic.GetEqualityExpression(model.GetId()).Compile();
		matches(model).Should().BeTrue();
		matches(ProductCharacteristic.Create(productId + 1, characteristicId, value)).Should().BeFalse();
		matches(ProductCharacteristic.Create(productId, characteristicId + 1, value)).Should().BeFalse();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("\t\r\n")]
	public void Create_EmptyOrWhitespaceValue_Throws(string value)
	{
		var act = () => ProductCharacteristic.Create(1, 10, value);

		act.Should().Throw<InvalidInputException>()
			.Which.LocalizableMessage.MessageKey.Should().Be(ArticleCharacteristicValueMustNotBeEmptyMessage.Key);
	}

	[Fact]
	public void Create_ValueLengthBoundary()
	{
		var value = new string('x', 128);
		ProductCharacteristic.Create(1, 10, $"  {value}  ").Value.Should().Be(value);
		var act = () => ProductCharacteristic.Create(1, 10, new string('x', 129));

		act.Should().Throw<InvalidInputException>()
			.Which.LocalizableMessage.MessageKey.Should().Be(ArticleCharacteristicValueMaxLengthMessage.Key);
	}

	[Theory]
	[InlineData("  NewValue  ", "NewValue")]
	[InlineData(" a ", "a")]
	[InlineData(" ab ", "ab")]
	public void SetValue_UpdatesValueAndPreservesKey(string input, string expected)
	{
		var model = ProductCharacteristic.Create(1, 10, "Value");

		model.SetValue(input);

		model.Value.Should().Be(expected);
		model.GetId().Should().Be((1, 10));
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void SetValue_InvalidValue_PreservesExistingValue(string value)
	{
		var model = ProductCharacteristic.Create(1, 10, "Value");
		var act = () => model.SetValue(value);

		act.Should().Throw<InvalidInputException>();
		model.Value.Should().Be("Value");
	}
}
