using Main.Enums;

namespace Main.Abstractions.Models.Pricing;


public record PriceCoefficient(string Name, int Order, decimal Value, CoefficientType Type, DateTime ValidTill);