namespace Main.Entities;

public partial class ArticleCoefficient
{
    public int ArticleId { get; set; }

    public string CoefficientName { get; set; } = null!;

    public DateTime ValidTill { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual Coefficient CoefficientNameNavigation { get; set; } = null!;
}
