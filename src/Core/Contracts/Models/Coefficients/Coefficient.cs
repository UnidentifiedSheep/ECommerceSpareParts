using Enums;

namespace Contracts.Models.Coefficients;

public class Coefficient
{
    public string Name { get; set; } = null!;

    public int Order { get; set; }

    public decimal Value { get; set; }

    public CoefficientType Type { get; set; }
}