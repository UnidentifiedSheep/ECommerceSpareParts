using Enums;

namespace Contracts.Models.Coefficients;

public class Coefficient
{
    public string Name { get; set; } = null!;

    public decimal Value { get; set; }

    public CoefficientType Type { get; set; }
}