using Domain;
using Exceptions;

namespace Main.Entities.Product;

public class ProductImage : Entity<ProductImage, (int, string)>
{
    public int ProductId { get; private set; }

    public string Path { get; private set; } = null!;

    public string? Description { get; private set; }

    private ProductImage() { }

    private ProductImage(int productId, string path, string? description)
    {
        ProductId = productId;
        SetPath(path);
        SetDescription(description);
    }

    public static ProductImage Create(int productId, string path, string? description)
        => new(productId, path, description);

    public void SetPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidInputException("article.image.path.empty");

        path = path.Trim();

        if (!IsSupportedExtension(path))
            throw new InvalidInputException("article.image.invalid.extension");

        Path = path;
    }

    public void SetDescription(string? description)
    {
        description = description?.Trim();

        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description;
    }

    public override (int, string) GetId() => (ProductId, Path);

    private static bool IsSupportedExtension(string path)
    {
        var lower = path.ToLowerInvariant();
        return SupportedExtensions.Any(ext => lower.EndsWith(ext));
    }

    private static readonly string[] SupportedExtensions =
    [
        ".png",
        ".jpeg",
        ".jpg",
        ".bmp",
        ".webp"
    ];
}