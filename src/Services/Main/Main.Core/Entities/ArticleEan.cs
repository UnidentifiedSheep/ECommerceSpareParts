namespace Main.Core.Entities;

public class ArticleEan
{
    public int ArticleId { get; set; }

    public string Ean { get; set; } = null!;

    public virtual Article Article { get; set; } = null!;
}