namespace Pricing.Abstractions.Models.Pricing;

/// <summary>
/// Result of a pricing calculation for a single item.
/// </summary>
/// <param name="Id">The unique identifier of the item.</param>
/// <param name="BasePrice">The base price of the item, price without applying any coefficients.</param>
/// <param name="FinalPrice">The final price after applying all valid coefficients to the base price.</param>
/// <param name="AppliedCoefficients">The collection of <see cref="PriceCoefficient"/> objects that were applied
/// to calculate the final price.
/// </param>
public record BasePricingItemResult(int Id, decimal BasePrice, decimal FinalPrice, IEnumerable<PriceCoefficient> AppliedCoefficients);