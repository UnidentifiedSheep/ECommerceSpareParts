namespace Main.Core.Entities;

public class ArticlesPair
{
    public int ArticleLeft { get; set; }

    public int ArticleRight { get; set; }

    public virtual Article ArticleLeftNavigation { get; set; } = null!;

    public virtual Article ArticleRightNavigation { get; set; } = null!;
}