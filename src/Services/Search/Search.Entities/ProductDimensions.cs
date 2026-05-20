using Enums;

namespace Search.Entities;

public class ProductDimensions
{
    public decimal Length { get; init; }

    public decimal Width { get; init; }

    public decimal Height { get; init; }

    public DimensionUnit Unit { get; init; }

    public decimal VolumeM3 { get; init; }
}
