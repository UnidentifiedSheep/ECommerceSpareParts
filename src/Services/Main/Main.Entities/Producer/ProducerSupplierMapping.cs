using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Enums;
using Main.Entities.DomainEvents.Producer;

namespace Main.Entities.Producer;

public class ProducerSupplierMapping : Entity<ProducerSupplierMapping, int>,
	ILinqEntity<ProducerSupplierMapping, int>
{
	private ProducerSupplierMapping()
	{
	}

	public int Id { get; private set; }

	public int ProducerId { get; private set; }

	public Supplier Supplier { get; private set; }

	public string Name { get; private set; } = string.Empty;

	public string NormalizedName { get; private set; } = string.Empty;

	public static Expression<Func<ProducerSupplierMapping, int>> GetKeySelector() => x => x.Id;

	public static Expression<Func<ProducerSupplierMapping, bool>> GetEqualityExpression(int key) => x =>
		x.Id == key;

	public static ProducerSupplierMapping Create(
		int producerId,
		string supplierProducerName,
		Supplier supplier)
	{
		return new ProducerSupplierMapping
		{
			ProducerId = producerId,
			Supplier = supplier,
			Name = supplierProducerName.Trim(),
			NormalizedName = Producer.ToNormalizedName(supplierProducerName)
		};
	}

	public override void OnCreated() => AddDomainEvent(new ProducerSupplierMappingChangedDomainEvent(ProducerId));

	public override void OnUpdated() => AddDomainEvent(new ProducerSupplierMappingChangedDomainEvent(ProducerId));

	public override void OnDeleted() => AddDomainEvent(new ProducerSupplierMappingChangedDomainEvent(ProducerId));

	public override int GetId() => Id;
}
