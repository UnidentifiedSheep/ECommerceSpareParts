using System.Linq.Expressions;
using Domain;
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

	private ProductImage()
	{
	}

	private ProductImage(int productId, string storageKey)
	{
		ProductId = productId;
		StorageKey = storageKey;
	}

	public int ProductId { get; }

	public string StorageKey { get; private set; } = null!;

	public static Expression<Func<ProductImage, (int, string)>> GetKeySelector() => x =>
		ValueTuple.Create(x.ProductId, x.StorageKey);

	public static Expression<Func<ProductImage, bool>> GetEqualityExpression((int, string) key) => x =>
		x.ProductId == key.Item1 && x.StorageKey == key.Item2;

	public static ProductImage Create(int productId, string extension)
	{
		IsSupportedExtension(extension, out var normalizedExtension)
			.EnsureTrue(ArticleImageInvalidExtensionMessage.Instance);

		return new ProductImage(productId, $"products/{productId}/{Guid.NewGuid():N}{normalizedExtension}");
	}

	public static ProductImage CreateFromStorageKey(int productId, string storageKey)
	{
		storageKey.EnsureNotNullOrWhiteSpace(ArticleImageInvalidStorageKeyMessage.Instance);
		var fullStorageKey = $"products/{productId}/{storageKey}"
			.EnsureMaxLength(255, ArticleImageInvalidStorageKeyMessage.Instance);

		(!storageKey.Contains('\\')
		 && !storageKey.Any(char.IsControl)
		 && storageKey.Split('/').All(segment => !string.IsNullOrWhiteSpace(segment) && segment is not "." and not ".."))
			.EnsureTrue(ArticleImageInvalidStorageKeyMessage.Instance);
		IsSupportedExtension(Path.GetExtension(storageKey), out _)
			.EnsureTrue(ArticleImageInvalidExtensionMessage.Instance);

		return new ProductImage(productId, fullStorageKey);
	}

	public override void OnCreated() => AddDomainEvent(new ProductImageUpdatedDomainEvent(ProductId));

	public override void OnUpdated() => OnCreated();

	public override void OnDeleted() => AddDomainEvent(new ProductImageDeletedDomainEvent(ProductId, StorageKey));

	public override (int, string) GetId() => (ProductId, StorageKey);

	private static bool IsSupportedExtension(string extension, out string normalizedExtension)
	{
		var normalized = extension.Trim().ToLowerInvariant();
		normalizedExtension = normalized.StartsWith('.') ? normalized : "." + normalized;

		return SupportedExtensions.Contains(normalizedExtension);
	}
}
