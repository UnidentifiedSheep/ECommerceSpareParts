using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Pricing.Enums;

namespace Pricing.Entities;

public class PriceOffer : Entity<PriceOffer, Guid>, ILinqEntity<PriceOffer, Guid>
{

    private PriceOffer() { }
    public Guid Id { get; private set; }
    public int ProductId { get; private set; }
    public int CurrencyId { get; private set; }

    public decimal Price { get; private set; }

    public PriceOfferSource Source { get; private set; }
    public string SourceKey { get; private set; } = string.Empty;

    public int AvailableQuantity { get; private set; }
    public int MinimumPurchaseQuantity { get; private set; }
    public int QuantityCoefficient { get; private set; }

    public int DaysToRefund { get; private set; }

    public DateTime DeliveryDate { get; private set; }
    public DateTime GuaranteedDeliveryDate { get; private set; }
    public int DeliveryProbability { get; private set; }
    public DateTime OrderTill { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public static Expression<Func<PriceOffer, Guid>> GetKeySelector()
    {
        return x => x.Id;
    }
    public static Expression<Func<PriceOffer, bool>> GetEqualityExpression(Guid key)
    {
        return x => x.Id == key;
    }

    public static PriceOffer Create(
        int productId,
        int currencyId,
        decimal price,
        PriceOfferSource source,
        string sourceKey,
        int availableQuantity,
        int minimumPurchaseQuantity,
        int quantityCoefficient,
        int daysToRefund,
        DateTime deliveryDate,
        DateTime guaranteedDeliveryDate,
        int deliveryProbability,
        DateTime orderTill,
        DateTime expiresAt)
    {
        return new PriceOffer
        {
            ProductId = productId,
            CurrencyId = currencyId,
            Price = price,
            Source = source,
            SourceKey = sourceKey,
            AvailableQuantity = availableQuantity,
            MinimumPurchaseQuantity = minimumPurchaseQuantity,
            QuantityCoefficient = quantityCoefficient,
            DaysToRefund = daysToRefund,
            DeliveryDate = deliveryDate,
            GuaranteedDeliveryDate = guaranteedDeliveryDate,
            DeliveryProbability = deliveryProbability,
            OrderTill = orderTill,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
    }
    public override Guid GetId()
    {
        return Id;
    }
}