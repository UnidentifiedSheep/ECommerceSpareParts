using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;
using Main.Entities.DomainEvents.Product;

namespace Main.Entities.Product;

public class ProductImage : Entity<ProductImage, (int, string)>, ILinqEntity<ProductImage, (int, string)>
{
    private static readonly HashSet<string> SupportedExtensions =
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
        string extension)
    {
        ProductId = productId;
        SetPath(extension);
    }

    public int ProductId { get; }

    public string StorageKey { get; private set; } = null!;

    public static Expression<Func<ProductImage, (int, string)>> GetKeySelector()
    {
        return x => ValueTuple.Create(x.ProductId, x.StorageKey);
    }

    public static Expression<Func<ProductImage, bool>> GetEqualityExpression((int, string) key)
    {
        return x => x.ProductId == key.Item1 && x.StorageKey == key.Item2;
    }

    public static ProductImage Create(
        int productId,
        string extension)
    {
        return new ProductImage(
            productId,
            extension);
    }

    private void SetPath(string extension)
    {
        IsSupportedExtension(extension, out var normalizedExtension)
            .EnsureTrue("article.image.invalid.extension");

        StorageKey = $"products/{ProductId}_{Guid.NewGuid():N}{normalizedExtension}";
    }

    public override void OnCreated() => AddDomainEvent(new ProductImageUpdatedDomainEvent(ProductId));
    public override void OnUpdated() => OnCreated();
    public override void OnDeleted() => OnCreated();

    public override (int, string) GetId() { return (ProductId, StorageKey); }

    private static bool IsSupportedExtension(string extension, out string normalizedExtension)
    {
        var normalized = extension.Trim().ToLowerInvariant();
        normalizedExtension = normalized.StartsWith('.')
            ? normalized
            : "." + normalized;

        return SupportedExtensions.Contains(normalizedExtension);
    }
}
