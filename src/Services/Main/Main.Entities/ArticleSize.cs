using BulkValidation.Core.Attributes;
using Main.Enums;

namespace Main.Entities;

public partial class ArticleSize
{
    [Validate]
    public int ArticleId { get; set; }

    public decimal Length { get; set; }

    public decimal Width { get; set; }

    public decimal Height { get; set; }

    public DimensionUnit Unit { get; set; }

    public decimal VolumeM3 { get; set; }

    public virtual Article Article { get; set; } = null!;
}
