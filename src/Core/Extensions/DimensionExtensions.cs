using Enums;

namespace Extensions;

public static class DimensionExtensions
{
    public static decimal ToCubicMeters(decimal length, decimal width, decimal height, DimensionUnit unit)
        => length.ToMeters(unit) * width.ToMeters(unit) * height.ToMeters(unit);
    
    public static decimal ToCubicMeters(decimal length, decimal width, decimal height)
        => length * width * height;

    public static decimal ToMeters(this decimal value, DimensionUnit unit)
    {
        return unit switch
        {
            DimensionUnit.Millimeter => value / 1000,
            DimensionUnit.Centimeter => value / 100,
            DimensionUnit.Meter => value,
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
        };
    }
}