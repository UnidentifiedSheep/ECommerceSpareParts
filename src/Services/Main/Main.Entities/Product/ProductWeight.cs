using BulkValidation.Core.Attributes;
using Enums;

namespace Main.Entities.Product;

public class ProductWeight
{
    [Validate]
    public int ProductId { get; set; }

    public decimal Weight { get; set; }

    public WeightUnit Unit { get; set; }
}