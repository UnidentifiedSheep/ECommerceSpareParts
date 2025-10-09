namespace Core.Entities;

public partial class ArticlesContent
{
    public int MainArticleId { get; set; }

    public int InsideArticleId { get; set; }

    public int Quantity { get; set; }

    public virtual Article InsideArticle { get; set; } = null!;

    public virtual Article MainArticle { get; set; } = null!;
}
