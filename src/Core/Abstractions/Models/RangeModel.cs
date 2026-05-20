namespace Abstractions.Models;

public sealed record RangeModel<T>(T? Min, T? Max)
    where T : struct, IComparable<T>
{
    public bool HasBounds => Min.HasValue || Max.HasValue;

    public static implicit operator RangeModel<T>((T Min, T Max) range)
    {
        return new RangeModel<T>(range.Min, range.Max);
    }

    public static implicit operator RangeModel<T>((T? Min, T? Max) range)
    {
        return new RangeModel<T>(range.Min, range.Max);
    }
}
