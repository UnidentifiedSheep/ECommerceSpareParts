using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Main.Entities.DomainEvents.CatalogueCandidate;
using Main.Entities.Product.ValueObjects;

namespace Main.Entities.Product.Enrichment;

public class CatalogueCandidate : AuditableEntity<CatalogueCandidate, Guid>,
	ILinqEntity<CatalogueCandidate, Guid>,
	IGenerateAutomaticDomainEvents
{

	private readonly List<SupplierProduct> _supplierProducts = [];

	private CatalogueCandidate()
	{
	}

	private CatalogueCandidate(string sku, int producerId)
	{
		Id = Guid.CreateVersion7();
		Sku = new Sku(sku);
		ProducerId = producerId;
	}

	public Guid Id { get; }

	public Sku Sku { get; private set; } = null!;

	public int ProducerId { get; private set; }

	public int? ProductId { get; private set; }

	public Product? Product { get; private set; }

	public Producer.Producer Producer { get; private set; } = null!;

	public IReadOnlyList<SupplierProduct> SupplierProducts => _supplierProducts;

	public static Expression<Func<CatalogueCandidate, Guid>> GetKeySelector() => x => x.Id;

	public static Expression<Func<CatalogueCandidate, bool>> GetEqualityExpression(Guid key) => x =>
		x.Id == key;

	public static CatalogueCandidate Create(string sku, int producerId)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(producerId);
		return new CatalogueCandidate(sku, producerId);
	}

	public void MapToProduct(int productId)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId);
		ProductId = productId;
	}

	public void RemoveProductMapping() => ProductId = null;

	public void AddSupplierProduct(SupplierProduct supplierProduct)
	{
		ArgumentNullException.ThrowIfNull(supplierProduct);

		if (supplierProduct.CatalogueCandidateId.HasValue && supplierProduct.CatalogueCandidateId != Id)
			throw new InvalidOperationException(
				"Supplier product is already assigned to another catalogue candidate.");

		if (_supplierProducts.Contains(supplierProduct))
			return;

		_supplierProducts.Add(supplierProduct);
		AddContentChangedDomainEvent();
	}

	public void RemoveSupplierProduct(SupplierProduct supplierProduct)
	{
		ArgumentNullException.ThrowIfNull(supplierProduct);

		if (!_supplierProducts.Remove(supplierProduct))
			return;

		AddContentChangedDomainEvent();
	}

	private void AddContentChangedDomainEvent() =>
		AddDomainEvent(new CatalogueCandidateContentChangedDomainEvent(Id));

	public override Guid GetId() => Id;
}
