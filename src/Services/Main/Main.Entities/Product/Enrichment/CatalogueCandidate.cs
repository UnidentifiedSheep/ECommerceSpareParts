using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Main.Entities.Product.ValueObjects;

namespace Main.Entities.Product.Enrichment;

public class CatalogueCandidate :
    AuditableEntity<CatalogueCandidate, int>,
    ILinqEntity<CatalogueCandidate, int>
{
    private CatalogueCandidate() { }

    private CatalogueCandidate(
        string sku,
        int producerId)
    {
        Sku = new Sku(sku);
        ProducerId = producerId;
    }

    public int Id { get; private set; }

    public Sku Sku { get; private set; } = null!;

    public int ProducerId { get; private set; }

    public int? ProductId { get; private set; }

    private readonly List<SupplierProduct> _supplierProducts = [];
    public IReadOnlyList<SupplierProduct> SupplierProducts => _supplierProducts;

    public static CatalogueCandidate Create(
        string sku,
        int producerId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(producerId);
        return new CatalogueCandidate(sku, producerId);
    }

    public void MapToProduct(int productId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId);
        ProductId = productId;
    }

    public void RemoveProductMapping()
    {
        ProductId = null;
    }

    public override int GetId() => Id;

    public static Expression<Func<CatalogueCandidate, int>> GetKeySelector() 
        => x => x.Id;

    public static Expression<Func<CatalogueCandidate, bool>> GetEqualityExpression(int key) 
        => x => x.Id == key;
}
