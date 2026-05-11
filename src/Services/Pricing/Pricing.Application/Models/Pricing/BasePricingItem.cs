namespace Pricing.Abstractions.Models.Pricing;

public record BasePricingItem(int Id, IEnumerable<ProductPrice> Prices, IEnumerable<PriceCoefficient> Coefficients);