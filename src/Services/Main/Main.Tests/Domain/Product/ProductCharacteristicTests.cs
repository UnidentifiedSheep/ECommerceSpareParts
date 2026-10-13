using Exceptions;
using FluentAssertions;
using Main.Entities.Product;

namespace Tests.Domain.Product;

public class ProductCharacteristicTests
{
	[Theory]
	[InlineData(1, 10, "Red")]
	[InlineData(2, 20, "  Large  ")]
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
	[InlineData("a")]
	[InlineData("ab")]
	public void Create_ValueTooShort_Throws(string value)
	{
		var act = () => ProductCharacteristic.Create(1, 10, value);

		act.Should().Throw<InvalidInputException>();
	}

	[Fact]
	public void Create_ValueTooLong_Throws()
	{
		var act = () => ProductCharacteristic.Create(1, 10, new string('x', 129));

		act.Should().Throw<InvalidInputException>();
	}

	[Fact]
	public void SetValue_UpdatesValueAndPreservesKey()
	{
		var model = ProductCharacteristic.Create(1, 10, "Value");

		model.SetValue("  NewValue  ");

		model.Value.Should().Be("NewValue");
		model.GetId().Should().Be((1, 10));
	}

	[Theory]
	[InlineData("ab")]
	[InlineData("   ")]
	public void SetValue_InvalidValue_PreservesExistingValue(string value)
	{
		var model = ProductCharacteristic.Create(1, 10, "Value");
		var act = () => model.SetValue(value);

		act.Should().Throw<InvalidInputException>();
		model.Value.Should().Be("Value");
	}
}
