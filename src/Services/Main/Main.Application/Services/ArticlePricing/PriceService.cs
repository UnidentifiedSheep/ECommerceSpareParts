using Main.Abstractions.Interfaces.Pricing;
using Main.Abstractions.Models.Pricing;
using Main.Application.Extensions;

namespace Main.Application.Services.ArticlePricing;

public class PriceService(IMarkupService markupService, IDiscountService discountService) : IPriceService
{
    public PricingResult GetPrice(PricingContext context)
    {
        decimal basePrice = context.BasePrice;
        decimal discount = context.Discount;
        int currencyId = context.CurrencyId;
        
        decimal markup = markupService.GetMarkup(basePrice, currencyId);
        decimal priceWithMarkup = basePrice.GetMarkUppedPrice(markup);
        decimal finalPrice = priceWithMarkup.GetDiscountedPrice(discount);
        
        return new PricingResult(basePrice, priceWithMarkup, finalPrice, markup, discount, currencyId);
    }
}