namespace Main.Entities;

public partial class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public int ArticleId { get; set; }

    public int Count { get; set; }

    public string SignedPrice { get; set; } = null!;

    public decimal? LockedPrice { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
