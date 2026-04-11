namespace Main.Entities.Product;

public class ProductImage
{
    public int ProductId { get; set; }
    
    public string Path { get; set; } = null!;

    public string? Description { get; set; }
}