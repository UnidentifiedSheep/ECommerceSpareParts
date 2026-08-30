using Enums;
using FluentAssertions;
using Main.Entities.Producer;

namespace Tests.Domain.Producer;

public sealed class ProducerSupplierMappingTests
{
	[Fact]
	public void Create_TrimsSupplierProducerName()
	{
		var mapping = ProducerSupplierMapping.Create(
			1,
			"  Bosch  ",
			Supplier.Armtek);

		mapping.SupplierProducerName.Should().Be("Bosch");
	}
}
