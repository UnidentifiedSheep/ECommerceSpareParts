using Enums;
using FluentAssertions;
using Main.Application.Models.Producer;

namespace Tests.ServicesTests;

public class SupplierProducerLookupTests
{
    private static readonly ProducerLookup ProducerLookup = new(
        new Dictionary<string, int>
        {
            ["BOSCH"] = 1
        },
        new Dictionary<string, int>
        {
            ["ROBERT BOSCH"] = 1
        });

    [Fact]
    public void ResolveId_PrefersExactSupplierMapping()
    {
        var lookup = CreateLookup(
            new SupplierProducerLookupKey(Supplier.Armtek, "Bosch"),
            2);

        lookup.ResolveId(Supplier.Armtek, " Bosch ")
            .Should().Be(2);
    }

    [Fact]
    public void ResolveId_DoesNotUseMappingFromAnotherSupplier()
    {
        var lookup = CreateLookup(
            new SupplierProducerLookupKey(Supplier.Armtek, "Bosch"),
            2);

        lookup.ResolveId(Supplier.Tmtr, "Bosch")
            .Should().Be(1);
    }

    [Theory]
    [InlineData("bosch")]
    [InlineData("Robert Bosch")]
    public void ResolveId_FallsBackToNormalizedProducerLookup(string producer)
    {
        var lookup = CreateLookup();

        lookup.ResolveId(Supplier.Armtek, producer)
            .Should().Be(1);
    }

    [Fact]
    public void ResolveId_ReturnsNullWhenProducerIsUnknown()
    {
        var lookup = CreateLookup();

        lookup.ResolveId(Supplier.Armtek, "Unknown")
            .Should().BeNull();
    }

    private static SupplierProducerLookup CreateLookup(
        SupplierProducerLookupKey? key = null,
        int producerId = 0)
    {
        IReadOnlyDictionary<SupplierProducerLookupKey, int> mappings = key is null
            ? new Dictionary<SupplierProducerLookupKey, int>()
            : new Dictionary<SupplierProducerLookupKey, int>
            {
                [key.Value] = producerId
            };

        return new SupplierProducerLookup(ProducerLookup, mappings);
    }
}
