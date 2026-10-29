using Enums;
using FluentAssertions;
using Main.Entities.Producer;

namespace Tests.Domain.Producer;

public sealed class ProducerSupplierMappingTests
{
	[Theory]
	[InlineData("  Bosch  ", "Bosch", "BOSCH")]
	[InlineData(" Robert Bosch ", "Robert Bosch", "ROBERT BOSCH")]
	[InlineData(" производитель ", "производитель", "ПРОИЗВОДИТЕЛЬ")]
	public void Create_PreservesSupplierNameAndNormalizesLookupName(string input, string original, string normalized)
	{
		var mapping = ProducerSupplierMapping.Create(
			1,
			input,
			Supplier.Armtek);

		mapping.Name.Should().Be(original);
		mapping.NormalizedName.Should().Be(normalized);
	}
}
