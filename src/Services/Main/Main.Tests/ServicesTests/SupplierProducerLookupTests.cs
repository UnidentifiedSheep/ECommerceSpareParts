using Enums;
using FluentAssertions;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Producer;

namespace Tests.ServicesTests;

public class ProducerLookupSupplierResolutionTests
{
	[Fact]
	public void ResolveId_PrefersExactSupplierMapping()
	{
		var lookup = CreateLookup(new ProducerSupplierLookupKey(Supplier.Armtek, "Bosch"), 2);

		lookup.ResolveId(" Bosch ", Supplier.Armtek).Should().Be(2);
	}

	[Fact]
	public void ResolveId_DoesNotUseMappingFromAnotherSupplier()
	{
		var lookup = CreateLookup(new ProducerSupplierLookupKey(Supplier.Armtek, "Bosch"), 2);

		lookup.ResolveId("Bosch", Supplier.Tmtr).Should().Be(1);
	}

	[Theory]
	[InlineData("bosch")]
	[InlineData("Robert Bosch")]
	public void ResolveId_FallsBackToNormalizedProducerLookup(string producer)
	{
		var lookup = CreateLookup();

		lookup.ResolveId(producer, Supplier.Armtek).Should().Be(1);
	}

	[Fact]
	public void ResolveId_ReturnsNullWhenProducerIsUnknown()
	{
		var lookup = CreateLookup();

		lookup.ResolveId("Unknown", Supplier.Armtek).Should().BeNull();
	}

	private static IProducerLookup CreateLookup(ProducerSupplierLookupKey? key = null, int producerId = 0)
	{
		IReadOnlyDictionary<ProducerSupplierLookupKey, int> mappings = key is null
			? new Dictionary<ProducerSupplierLookupKey, int>()
			: new Dictionary<ProducerSupplierLookupKey, int>
			{
				[key.Value] = producerId
			};

		IProducerLookup lookup = new ProducerLookup(
			new Dictionary<string, int>
			{
				["BOSCH"] = 1
			},
			new Dictionary<string, int>
			{
				["ROBERT BOSCH"] = 1
			});

		return new SupplierProducerLookup(lookup, mappings);
	}
}
