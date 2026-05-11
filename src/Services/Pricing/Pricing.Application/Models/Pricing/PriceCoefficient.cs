using Enums;

namespace Pricing.Abstractions.Models.Pricing;

public record PriceCoefficient(string Name, decimal Value, CoefficientType Type, DateTime ValidTill);