using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;

namespace Pricing.Entities;

public class PriceRecalculationRequest 
    : Entity<PriceRecalculationRequest, PriceRecalculationRequestKey>,
        ILinqEntity<PriceRecalculationRequest, PriceRecalculationRequestKey>
{
    private PriceRecalculationRequest() { }
    public int ProductId { get; private set; }
    public string StorageName { get; private set; } = string.Empty;
    public DateTime UpdatedAt { get; private set; }

    public static PriceRecalculationRequest Create(int productId, string storageName)
    {
        return new PriceRecalculationRequest
        {
            StorageName = storageName,
            ProductId = productId,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    public override PriceRecalculationRequestKey GetId() => new(ProductId, StorageName);
    public static Expression<Func<PriceRecalculationRequest, PriceRecalculationRequestKey>> GetKeySelector()
        => x => new PriceRecalculationRequestKey(x.ProductId, x.StorageName);
    public static Expression<Func<PriceRecalculationRequest, bool>> GetEqualityExpression(PriceRecalculationRequestKey key)
        => x => x.ProductId == key.ProductId && x.StorageName == key.StorageName;
}

public readonly struct PriceRecalculationRequestKey(int productId, string storageName) : ICompositeKey
{
    public int ProductId => productId;
    public string StorageName => storageName;
    public object[] ToArray() => [productId, storageName];
}