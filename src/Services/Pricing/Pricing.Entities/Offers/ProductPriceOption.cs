using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Pricing.Entities.Offers;

public class ProductPriceOption : AuditableEntity<ProductPriceOption, Guid>, ILinqEntity<ProductPriceOption, Guid>
{
    private ProductPriceOption() { }
    public Guid PriceOfferId { get; private set; }
    
    public decimal Score { get; private set; }
    public int CurrencyId { get; private set; }
    public decimal Price { get; private set; }
    public decimal Markup { get; private set; }
    
    public string ForStorageName { get; private set; } = string.Empty;
    public TimeSpan DeliveryTime { get; private set; }
    public TimeSpan GuaranteedDeliveryTime { get; private set; }
    public int DeliveryProbability { get; private set; }
    
    public string MarkupVersion { get; private set; } = string.Empty;
    public string AppliersVersion { get; private set; } = string.Empty;
    public Guid PricingSettingsVersion { get; private set; }
    
    public PriceOffer PriceOffer { get; private set; } = null!;

    public static ProductPriceOption Create(
        Guid priceOfferId,
        string markupVersion,
        string appliersVersion,
        Guid pricingSettingsVersion,
        string storageName,
        decimal score,
        decimal price,
        int currencyId,
        decimal markup,
        TimeSpan deliveryTime,
        TimeSpan guaranteedDeliveryTime,
        int deliveryProbability)
    {
        var option = new ProductPriceOption
        {
            PriceOfferId = priceOfferId,
            CurrencyId = currencyId
        };
        option.SetStorageName(storageName);
        option.SetScore(score);
        option.SetPrice(price);
        option.SetMarkup(markup);
        option.SetDeliveryTime(deliveryTime, guaranteedDeliveryTime, deliveryProbability);
        option.MarkupVersion = markupVersion;
        option.AppliersVersion = appliersVersion;
        option.PricingSettingsVersion = pricingSettingsVersion;
        return option;
    }
    
    public void SetScore(decimal score) 
        => Score = score.EnsureNonNegative(() => new InvalidOperationException("Score cannot be negative"));

    public void SetPrice(decimal priceInBaseCurrency)
        => Price = priceInBaseCurrency
            .EnsureGreaterThan(
                min: 0, 
                exceptionFactory: () => new InvalidOperationException("Price cannot be negative"));
    
    public void SetMarkup(decimal markup)
        => Markup = markup.EnsureNonNegative(
            () => new InvalidOperationException("Markup cannot be negative"));

    public void SetDeliveryTime(
        TimeSpan deliveryTime,
        TimeSpan guaranteedDeliveryTime,
        int deliveryProbability)
    {
        deliveryProbability.EnsureNonNegative(() => new InvalidOperationException("Delivery probability cannot be negative"));
        deliveryTime.EnsureAtLeast(
            min: TimeSpan.Zero, 
            exceptionFactory: () => new InvalidOperationException("Delivery time cannot be negative"));
        guaranteedDeliveryTime.EnsureAtLeast(
            min: TimeSpan.Zero, 
            exceptionFactory: () => new InvalidOperationException("Guaranteed delivery time cannot be negative"));
        
        DeliveryTime = deliveryTime;
        GuaranteedDeliveryTime = guaranteedDeliveryTime;
        DeliveryProbability = deliveryProbability;
    }
    
    public void SetStorageName(string storageName) => ForStorageName = storageName
        .TrimSafe()
        .EnsureNotNullOrWhiteSpace(() => new InvalidOperationException("Storage name cannot be empty"));
    
    public override Guid GetId() => PriceOfferId;
    public static Expression<Func<ProductPriceOption, Guid>> GetKeySelector() => x => x.PriceOfferId;
    public static Expression<Func<ProductPriceOption, bool>> GetEqualityExpression(Guid key) => x => x.PriceOfferId == key;
}
