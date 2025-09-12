namespace Core.Entities;

public class ArticleImage
{
    public int ArticleId { get; set; }

    public string Path { get; set; } = null!;

    public string? Description { get; set; }

    public virtual Article Article { get; set; } = null!;
}