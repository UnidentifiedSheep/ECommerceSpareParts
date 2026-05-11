using Enums;

namespace Pricing.Abstractions.Models.Pricing;

public record PriceCoefficient(string Name, int Order, decimal Value, CoefficientType Type, DateTime ValidTill);