using Enums;
using Enums.Units;

namespace Search.Entities;

public class ProductDimensions
{
    public decimal Length { get; init; }

    public decimal LengthM { get; init; }

    public decimal Width { get; init; }

    public decimal WidthM { get; init; }

    public decimal Height { get; init; }

    public decimal HeightM { get; init; }

    public DimensionUnit Unit { get; init; }

    public decimal VolumeM3 { get; init; }
}