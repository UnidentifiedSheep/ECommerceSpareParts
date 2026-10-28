using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;

namespace Main.Application.Lrts.DeleteProductImages;

public record DeleteProductImagesInputState : IInputState
{
	[JsonPropertyName("productId")]
	public required int ProductId { get; init; }

	[JsonPropertyName("bucketName")]
	public required string BucketName { get; init; }

	[JsonPropertyName("storageKeys")]
	public required string[] StorageKeys { get; init; }

	public void ValidateState()
	{
		if (ProductId <= 0)
			throw new InvalidOperationException("Product ID must be greater than zero.");

		if (string.IsNullOrWhiteSpace(BucketName))
			throw new InvalidOperationException("Bucket name is required.");

		if (StorageKeys.Length == 0 || StorageKeys.Any(key =>
			string.IsNullOrWhiteSpace(key) || !key.StartsWith("products/", StringComparison.Ordinal)
			|| key.Contains('\\') || key.Any(char.IsControl)
			|| key.Split('/').Any(segment => string.IsNullOrWhiteSpace(segment) || segment is "." or "..")))
			throw new InvalidOperationException("Invalid product image keys.");
	}
}
