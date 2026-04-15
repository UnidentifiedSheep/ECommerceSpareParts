using Domain;
using Exceptions;

namespace Main.Entities.Product;

public class ProductImage : Entity<ProductImage, (int, string)>
{
    public int ProductId { get; private set; }
    
    public string Path { get; private set; } = null!;

    public string? Description { get; private set; }
    
    private ProductImage() {}

    private ProductImage(int productId, string path, string? description)
    {
        ProductId = productId;
        SetPath(path);
        SetDescription(description);
    }

    public static ProductImage Create(int productId, string path, string? description)
    {
        return new ProductImage(productId, path, description);
    }

    public void SetPath(string path)
    {
        path = path.Trim();
        if (!SupportedExtensions.Any(path.EndsWith))
            throw new InvalidInputException("article.image.invalid.extension");
        
        Path = path;
    }

    public void SetDescription(string? description)
    {
        description = description?.Trim();
        Description = string.IsNullOrEmpty(description)
            ? null
            : description;
    }
    
    public override (int, string) GetId() => (ProductId, Path);
    
    private static readonly string[] SupportedExtensions =
    [
        ".png",
        ".jpeg",
        ".jpg",
        ".bmp",
        ".webp"
    ];
}