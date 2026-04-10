namespace Main.Entities;

public class ArticleCharacteristic
{
    public int Id { get; set; }

    public int ArticleId { get; set; }

    public string Value { get; set; } = null!;

    public string? Name { get; set; }

    public virtual Product Product { get; set; } = null!;
}