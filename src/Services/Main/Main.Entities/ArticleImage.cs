namespace Main.Entities;

public class ArticleImage
{
    public string Path { get; set; } = null!;

    public int ArticleId { get; set; }

    public string? Description { get; set; }

    public virtual Product Product { get; set; } = null!;
}