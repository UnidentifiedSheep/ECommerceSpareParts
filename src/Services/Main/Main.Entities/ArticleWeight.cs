using BulkValidation.Core.Attributes;
using Enums;
using Main.Enums;

namespace Main.Entities;

public partial class ArticleWeight
{
    [Validate]
    public int ArticleId { get; set; }

    public decimal Weight { get; set; }

    public WeightUnit Unit { get; set; }

    public virtual Article Article { get; set; } = null!;
}
