using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Pricing.Entities;

public class ProductPriceOption : AuditableEntity<ProductPriceOption, Guid>, ILinqEntity<ProductPriceOption, Guid>
{
    private ProductPriceOption() { }
    public Guid PriceOfferId { get; private set; }
    
    public decimal Score { get; private set; }
    public int CurrencyId { get; private set; }
    public decimal Price { get; private set; }
    public decimal Markup { get; private set; }
    
    public TimeSpan DeliveryTime { get; private set; }
    public TimeSpan GuaranteedDeliveryTime { get; private set; }
    public int DeliveryProbability { get; private set; }

    public static ProductPriceOption Create(
        Guid priceOfferId,
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
        option.SetScore(score);
        option.SetPrice(price);
        option.SetMarkup(markup);
        option.SetDeliveryTime(deliveryTime, guaranteedDeliveryTime, deliveryProbability);
        return option;
    }
    
    public void SetScore(decimal score) 
        => Score = score.AgainstNegative(() => new InvalidOperationException("Score cannot be negative"));

    public void SetPrice(decimal priceInBaseCurrency)
        => Price = priceInBaseCurrency
            .AgainstLessOrEqual(
                min: 0, 
                exceptionFactory: () => new InvalidOperationException("Price cannot be negative"));
    
    public void SetMarkup(decimal markup)
        => Markup = markup.AgainstNegative(
            () => new InvalidOperationException("Markup cannot be negative"));

    public void SetDeliveryTime(
        TimeSpan deliveryTime,
        TimeSpan guaranteedDeliveryTime,
        int deliveryProbability)
    {
        deliveryProbability.AgainstNegative(() => new InvalidOperationException("Delivery probability cannot be negative"));
        deliveryTime.AgainstTooSmall(
            min: TimeSpan.Zero, 
            exceptionFactory: () => new InvalidOperationException("Delivery time cannot be negative"));
        guaranteedDeliveryTime.AgainstTooSmall(
            min: TimeSpan.Zero, 
            exceptionFactory: () => new InvalidOperationException("Guaranteed delivery time cannot be negative"));
        
        DeliveryTime = deliveryTime;
        GuaranteedDeliveryTime = guaranteedDeliveryTime;
        DeliveryProbability = deliveryProbability;
    }
    
    public override Guid GetId() => PriceOfferId;
    public static Expression<Func<ProductPriceOption, Guid>> GetKeySelector() => x => x.PriceOfferId;
    public static Expression<Func<ProductPriceOption, bool>> GetEqualityExpression(Guid key) => x => x.PriceOfferId == key;
}