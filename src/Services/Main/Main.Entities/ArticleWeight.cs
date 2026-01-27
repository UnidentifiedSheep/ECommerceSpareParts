using Main.Enums;

namespace Main.Entities;

public partial class ArticleWeight
{
    public int ArticleId { get; set; }

    public decimal Weight { get; set; }

    public WeightUnit Unit { get; set; }

    public virtual Article Article { get; set; } = null!;
}
