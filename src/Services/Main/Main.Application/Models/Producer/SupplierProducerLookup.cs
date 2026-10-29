using System.Collections.Frozen;
using Enums;
using Main.Application.Interfaces.Services;
using ProducerEntity = Main.Entities.Producer.Producer;

namespace Main.Application.Models.Producer;

public sealed class SupplierProducerLookup(
	IProducerLookup inner,
	IReadOnlyDictionary<ProducerSupplierLookupKey, int> supplierMappings) : IProducerLookup
{
	private readonly FrozenDictionary<ProducerSupplierLookupKey, int> _supplierMappings = supplierMappings.ToFrozenDictionary();

	public int? ResolveId(string producer, Supplier? supplier = null)
	{
		if (string.IsNullOrWhiteSpace(producer))
			return null;

		if (supplier.HasValue)
		{
			var supplierKey = new ProducerSupplierLookupKey(supplier.Value, ProducerEntity.ToNormalizedName(producer));

			if (_supplierMappings.TryGetValue(supplierKey, out var producerId))
				return producerId;
		}

		return inner.ResolveId(producer);
	}
}

public readonly record struct ProducerSupplierLookupKey(Supplier Supplier, string NormalizedName);
