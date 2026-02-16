namespace Pricing.Abstractions.Models.Pricing;

/// <summary>
/// Context for final price calculation.
/// </summary>
/// <param name="BasePrice">Base price</param>
/// <param name="Discount">Discount, for example, 0.2 which means 20%</param>
/// <param name="CurrencyId">The id of used currency.</param>
public record PricingContext(decimal BasePrice, decimal Discount, int CurrencyId);