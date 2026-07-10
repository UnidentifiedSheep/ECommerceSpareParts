namespace Pricing.Application.Models.Pricing;

public sealed record AppliedPriceRule(
    string Name,
    decimal PriceBefore,
    decimal PriceAfter);