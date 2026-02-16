using Pricing.Abstractions.Interfaces.Services.Pricing;
using Pricing.Abstractions.Models.Pricing;

namespace Pricing.Application.Services.ArticlePricing;

public class PriceService(IMarkupService markupService, IDiscountService discountService) : IPriceService
{
    public PricingResult GetPrice(PricingContext context)
    {
        decimal basePrice = context.BasePrice;
        decimal discount = context.Discount;
        int currencyId = context.CurrencyId;
        
        decimal markup = markupService.GetMarkup(basePrice, currencyId);
        decimal priceWithMarkup = markupService.WithMarkup(basePrice, markup);
        decimal finalPrice = discountService.WithDiscount(priceWithMarkup, discount);
        
        return new PricingResult(basePrice, priceWithMarkup, finalPrice, markup, discount, currencyId);
    }
}