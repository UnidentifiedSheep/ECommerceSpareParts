using BulkValidation.Core.Attributes;
using Enums;

namespace Main.Entities.Product;

public class ProductSize
{
    [Validate]
    public int ProductId { get; set; }

    public decimal Length { get; set; }

    public decimal Width { get; set; }

    public decimal Height { get; set; }

    public DimensionUnit Unit { get; set; }

    public decimal VolumeM3 { get; set; }
}