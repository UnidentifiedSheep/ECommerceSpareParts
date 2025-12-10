namespace Main.Core.Entities;

public partial class Cart
{
    public string Id { get; set; } = null!;

    public Guid UserId { get; set; }

    public int ArticleId { get; set; }

    public int Count { get; set; }

    public decimal Price { get; set; }

    public DateTime ValidTill { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
