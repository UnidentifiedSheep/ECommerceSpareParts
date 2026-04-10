namespace Main.Entities;

public class ArticlesPair
{
    public int ArticleLeft { get; set; }

    public int ArticleRight { get; set; }

    public virtual Product ProductLeftNavigation { get; set; } = null!;

    public virtual Product ProductRightNavigation { get; set; } = null!;
}