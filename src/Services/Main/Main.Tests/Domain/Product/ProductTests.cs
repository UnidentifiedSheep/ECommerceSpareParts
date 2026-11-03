using FluentAssertions;
using Main.Entities.Product.ValueObjects;
using ProductDomain = Main.Entities.Product.Product;

namespace Tests.Domain.Product;

public class ProductTests
{
	[Fact]
	public void Create_ValidData_Succeeds()
	{
		var product = ProductDomain.Create(
			new Sku("ABC-123"),
			new Name("test product"),
			1,
			"desc");

		product.Sku.Value.Should().Be("ABC-123");
		product.Name.Value.Should().Be("Test product");
		product.ProducerId.Should().Be(1);
		product.Description.Should().Be("desc");
	}

	[Fact]
	public void SetDescription_Normalizes()
	{
		var product = Create();

		product.SetDescription("  hello  ");

		product.Description.Should().Be("hello");
	}

	[Fact]
	public void SetDescription_EmptyBecomesNull()
	{
		var product = Create();

		product.SetDescription("   ");

		product.Description.Should().BeNull();
	}

	[Fact]
	public void AddAndRemoveEan_UpdatesCollectionWithoutDuplicates()
	{
		var product = Create();

		product.AddEan("1234567890123");
		product.AddEan("1234567890123");
		product.AddEan("1234567890124");

		product.Eans.Select(x => x.Ean).Should().BeEquivalentTo(["1234567890123", "1234567890124"]);

		product.RemoveEan("1234567890123");
		product.Eans.Select(x => x.Ean).Should().Equal("1234567890124");
	}

	[Fact]
	public void SetPackingUnit_Negative_Throws()
	{
		var product = Create();

		var act = () => product.SetPackingUnit(-1);

		act.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void SetPopularity_Negative_Throws()
	{
		var product = Create();

		var act = () => product.SetPopularity(-1);

		act.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void ValueObjects_WorkCorrectly()
	{
		var sku = new Sku("ABC-123");
		var name = new Name("test product");

		sku.NormalizedValue.Should().Be("ABC123");
		name.Value.Should().Be("Test product");
	}

	private static ProductDomain Create()
	{
		return ProductDomain.Create(
			new Sku("ABC-123"),
			new Name("test product"),
			1,
			"desc");
	}
}
