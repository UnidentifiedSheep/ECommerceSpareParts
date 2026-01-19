namespace Main.Entities;

public partial class ArticleImage
{
    public string Path { get; set; } = null!;

    public int ArticleId { get; set; }

    public string? Description { get; set; }

    public virtual Article Article { get; set; } = null!;
}
