namespace Main.Abstractions.Models.Pricing;

/// <summary>
/// Context for final price calculation.
/// </summary>
/// <param name="BasePrice">Base price</param>
/// <param name="Discount">Discount, for example, 20%</param>
/// <param name="CurrencyId">The id of used currency.</param>
public record PricingContext(decimal BasePrice, decimal Discount, int CurrencyId);