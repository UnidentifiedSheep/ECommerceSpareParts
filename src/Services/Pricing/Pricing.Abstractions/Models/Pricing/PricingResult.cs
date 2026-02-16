namespace Pricing.Abstractions.Models.Pricing;

public record PricingResult(decimal BasePrice, decimal PriceWithMarkup, decimal FinalPrice, decimal AppliedMarkup, 
    decimal Discount, int CurrencyId);