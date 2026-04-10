namespace Main.Entities;

public class ArticlesContent
{
    public int MainArticleId { get; set; }

    public int InsideArticleId { get; set; }

    public int Quantity { get; set; }

    public virtual Product InsideProduct { get; set; } = null!;

    public virtual Product MainProduct { get; set; } = null!;
}