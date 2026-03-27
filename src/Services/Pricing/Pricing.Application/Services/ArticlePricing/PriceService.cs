using Pricing.Abstractions.Interfaces.Services.Pricing;
using Pricing.Abstractions.Models.Pricing;

namespace Pricing.Application.Services.ArticlePricing;

public class PriceService(IMarkupService markupService, IDiscountService discountService) : IPriceService
{
    public PricingResult GetPrice(PricingContext context)
    {
        var basePrice = context.BasePrice;
        var discount = context.Discount;
        var currencyId = context.CurrencyId;

        var markup = markupService.GetMarkup(basePrice, currencyId);
        var priceWithMarkup = markupService.WithMarkup(basePrice, markup);
        var finalPrice = discountService.WithDiscount(priceWithMarkup, discount);

        return new PricingResult(basePrice, priceWithMarkup, finalPrice, markup, discount, currencyId);
    }
}