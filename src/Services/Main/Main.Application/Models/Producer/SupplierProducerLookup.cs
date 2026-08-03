using Enums;

namespace Main.Application.Models.Producer;

public sealed class SupplierProducerLookup(
    ProducerLookup producerLookup,
    IReadOnlyDictionary<SupplierProducerLookupKey, int> supplierMappings)
{
    public int? ResolveId(
        Supplier supplier,
        string producer)
    {
        if (string.IsNullOrWhiteSpace(producer)) return null;

        var supplierKey = new SupplierProducerLookupKey(
            supplier,
            producer.Trim());

        return supplierMappings.TryGetValue(supplierKey, out var producerId)
            ? producerId
            : producerLookup.ResolveId(producer);
    }
}

public readonly record struct SupplierProducerLookupKey(
    Supplier Supplier,
    string SupplierProducerName);
