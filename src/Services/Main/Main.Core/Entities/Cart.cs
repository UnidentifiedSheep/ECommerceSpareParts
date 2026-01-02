namespace Main.Core.Entities;

public partial class Cart
{
    public Guid UserId { get; set; }

    public int ArticleId { get; set; }

    public int Count { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
