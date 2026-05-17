using Enums;

namespace Pricing.Application.Models.Pricing;

public record PriceCoefficient(string Name, decimal Value, CoefficientType Type, DateTime ValidTill);