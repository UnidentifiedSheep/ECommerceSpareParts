using Enums;

namespace Extensions;

public static class WeightExtensions
{
    public static decimal ToKg(this decimal value, WeightUnit unit)
    {
        return unit switch
        {
            WeightUnit.Gram => value / 1000,
            WeightUnit.Kilogram => value,
            WeightUnit.Tonne => value * 1000,
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
        };
    }
}