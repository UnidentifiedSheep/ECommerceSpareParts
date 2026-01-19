using BulkValidation.Core.Attributes;

namespace Main.Entities;

public partial class Cart
{
    [ValidateTuple("PK")]
    public Guid UserId { get; set; }
    [ValidateTuple("PK")]
    public int ArticleId { get; set; }

    public int Count { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
