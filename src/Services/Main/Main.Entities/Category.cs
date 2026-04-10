namespace Main.Entities;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Articles { get; set; } = new List<Product>();
}