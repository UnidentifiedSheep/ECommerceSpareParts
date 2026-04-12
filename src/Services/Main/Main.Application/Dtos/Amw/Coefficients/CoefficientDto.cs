using Enums;

namespace Main.Abstractions.Dtos.Amw.Coefficients;

public class CoefficientDto
{
    public string Name { get; set; } = null!;

    public int Order { get; set; }

    public decimal Value { get; set; }

    public CoefficientType Type { get; set; }
}