using BulkValidation.Core.Attributes;
using Enums;

namespace Main.Entities;

public class ArticleWeight
{
    [Validate]
    public int ArticleId { get; set; }

    public decimal Weight { get; set; }

    public WeightUnit Unit { get; set; }

    public virtual Product Product { get; set; } = null!;
}