namespace Core.Entities;

public class ArticleCharacteristic
{
    public int Id { get; set; }

    public int ArticleId { get; set; }

    public string Value { get; set; } = null!;

    public string? Name { get; set; }

    public virtual Article Article { get; set; } = null!;
}