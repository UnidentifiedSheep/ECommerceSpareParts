using Enums;

namespace Search.Entities;

public class ProductWeight
{
    public decimal Value { get; init; }

    public WeightUnit Unit { get; init; }

    public decimal WeightKg { get; init; }
}
