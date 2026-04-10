namespace Main.Entities;

public class ArticleEan
{
    public int ArticleId { get; set; }

    public string Ean { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}