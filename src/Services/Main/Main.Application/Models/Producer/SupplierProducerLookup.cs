using Enums;
using Main.Application.Interfaces.Services;

namespace Main.Application.Models.Producer;

public sealed class SupplierProducerLookup(
	IProducerLookup inner,
	IReadOnlyDictionary<ProducerSupplierLookupKey, int> supplierMappings) : IProducerLookup
{
	public int? ResolveId(string producer, Supplier? supplier = null)
	{
		if (string.IsNullOrWhiteSpace(producer))
			return null;

		if (supplier.HasValue)
		{
			var supplierKey = new ProducerSupplierLookupKey(supplier.Value, producer.Trim());

			if (supplierMappings.TryGetValue(supplierKey, out var producerId))
				return producerId;
		}

		return inner.ResolveId(producer);
	}
}

public readonly record struct ProducerSupplierLookupKey(Supplier Supplier, string SupplierProducerName);
