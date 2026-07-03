using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Exceptions;
using Main.Entities.DomainEvents.Product;

namespace Main.Entities.Product;

public class ProductImage : Entity<ProductImage, (int, string)>, ILinqEntity<ProductImage, (int, string)>
{
    private static readonly string[] SupportedExtensions =
    [
        ".png",
        ".jpeg",
        ".jpg",
        ".bmp",
        ".webp"
    ];

    private ProductImage() { }

    private ProductImage(
        int productId,
        string path,
        string? description)
    {
        ProductId = productId;
        SetPath(path);
        SetDescription(description);
    }

    public int ProductId { get; }

    public string Path { get; private set; } = null!;

    public string? Description { get; private set; }

    public static Expression<Func<ProductImage, (int, string)>> GetKeySelector()
    {
        return x => ValueTuple.Create(x.ProductId, x.Path);
    }

    public static Expression<Func<ProductImage, bool>> GetEqualityExpression((int, string) key)
    {
        return x => x.ProductId == key.Item1 && x.Path == key.Item2;
    }

    public static ProductImage Create(
        int productId,
        string path,
        string? description)
    {
        return new ProductImage(
            productId,
            path,
            description);
    }

    public void SetPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new InvalidInputException("article.image.path.empty");

        path = path.Trim();

        if (!IsSupportedExtension(path)) throw new InvalidInputException("article.image.invalid.extension");

        Path = path;
    }

    public void SetDescription(string? description)
    {
        description = description?.Trim();

        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description;
    }

    public override void OnCreated() => AddDomainEvent(new ProductImageUpdatedDomainEvent(ProductId));
    public override void OnUpdated() => OnCreated();
    public override void OnDeleted() => OnCreated();

    public override (int, string) GetId() { return (ProductId, Path); }

    private static bool IsSupportedExtension(string path)
    {
        var lower = path.ToLowerInvariant();
        return SupportedExtensions.Any(lower.EndsWith);
    }
}