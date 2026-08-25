using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;
using Pricing.Enums;

namespace Pricing.Entities.Offers;

public class PriceOfferRefreshState 
    : Entity<PriceOfferRefreshState, PriceOfferRefreshStateKey>, 
        ILinqEntity<PriceOfferRefreshState, PriceOfferRefreshStateKey>
{
    private PriceOfferRefreshState() { }
    public int ProductId { get; private set; }
    public PriceOfferSource Source { get; private set; }
    public string StorageCode { get; private set; } = null!;

    public DateTime? LastOffersUpdatedAt { get; private set; }
    public int LastOffersCount { get; private set; }

    public static PriceOfferRefreshState Create(
        int productId,
        PriceOfferSource source,
        string storageCode)
    {
        return new PriceOfferRefreshState
        {
            ProductId = productId,
            Source = source,
            StorageCode = storageCode
        };
    }

    public void OffersUpdated(DateTime offersUpdatedAt, int offersCount)
    {
        LastOffersCount = offersCount.EnsureNonNegative(
            () => new InvalidOperationException("Offers count cannot be negative"));

        LastOffersUpdatedAt = offersUpdatedAt;
    }
    
    public override PriceOfferRefreshStateKey GetId() => new(ProductId, Source, StorageCode);
    public static Expression<Func<PriceOfferRefreshState, PriceOfferRefreshStateKey>> GetKeySelector()
        => x => new PriceOfferRefreshStateKey(x.ProductId, x.Source, x.StorageCode);
    public static Expression<Func<PriceOfferRefreshState, bool>> GetEqualityExpression(PriceOfferRefreshStateKey key)
        => x => x.ProductId == key.ProductId && x.Source == key.Supplier && x.StorageCode == key.StorageCode;
}

public readonly struct PriceOfferRefreshStateKey(int productId, PriceOfferSource source, string storageCode) : ICompositeKey
{
    public int ProductId => productId;
    public PriceOfferSource Supplier => source;
    public string StorageCode => storageCode;
    public object[] ToArray() => [productId, source, storageCode];
}