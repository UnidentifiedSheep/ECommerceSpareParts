namespace Main.Entities;

public class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public int ArticleId { get; set; }

    public int Count { get; set; }

    public string SignedPrice { get; set; } = null!;

    public decimal? LockedPrice { get; set; }

    public virtual Product.Product Product { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}