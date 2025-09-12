namespace Core.Entities;

public partial class Cart
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public int ArticleId { get; set; }

    public int Count { get; set; }

    public decimal Price { get; set; }

    public DateTime ValidTill { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
