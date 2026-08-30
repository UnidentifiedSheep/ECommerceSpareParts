using Abstractions.Interfaces;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Options.S3;
using Attributes;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using MediatR;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Products;

[Transactional]
[AutoSave]
public record RemoveProductImageCommand(int ProductId, string ImagePath) : ICommand;

public class RemoveProductImageHandler(
	IS3StorageService s3Storage,
	IUnitOfWork unitOfWork,
	IOptions<S3BucketsOptions> bucketsOptions,
	IRepository<ProductImage, (int, string)> repository) : ICommandHandler<RemoveProductImageCommand>
{
	public async Task<Unit> Handle(RemoveProductImageCommand request, CancellationToken cancellationToken)
	{
		var bucket = bucketsOptions.Value.Images;
		var storageKey = GetStorageKey(request.ImagePath, bucket);

		var imageEntity = await repository.GetById((request.ProductId, storageKey), cancellationToken) ??
			throw new ProductImageNotFoundException(request.ProductId, storageKey);

		unitOfWork.Remove(imageEntity);

		await s3Storage.DeleteFileAsync(bucket.Name, storageKey);

		return Unit.Value;
	}

	private static string GetStorageKey(string imagePath, BucketOptions bucket)
	{
		var normalized = NormalizeImagePath(imagePath);
		var path = Uri.TryCreate(
			normalized,
			UriKind.Absolute,
			out var imageUri)
			? imageUri.AbsolutePath
			: RemoveQueryAndFragment(normalized);

		path = Uri.UnescapeDataString(path).Trim('/');

		if (Uri.TryCreate(
				bucket.PublicBaseUrl,
				UriKind.Absolute,
				out var publicBaseUri))
		{
			var publicBasePath = Uri.UnescapeDataString(publicBaseUri.AbsolutePath).Trim('/');
			path = RemovePrefix(path, publicBasePath);
		}

		return RemovePrefix(path, bucket.Name.Trim('/'));
	}

	private static string NormalizeImagePath(string imagePath)
	{
		var normalized = imagePath.Trim();
		return normalized
			.Replace(
				"http//",
				"http://",
				StringComparison.OrdinalIgnoreCase)
			.Replace(
				"https//",
				"https://",
				StringComparison.OrdinalIgnoreCase);
	}

	private static string RemoveQueryAndFragment(string path)
	{
		var separatorIndex = path.IndexOfAny(['?', '#']);
		return separatorIndex >= 0 ? path[..separatorIndex] : path;
	}

	private static string RemovePrefix(string path, string prefix)
	{
		if (string.IsNullOrEmpty(prefix))
			return path;
		if (path.Equals(prefix, StringComparison.OrdinalIgnoreCase))
			return string.Empty;

		var prefixWithSeparator = prefix + "/";
		return path.StartsWith(prefixWithSeparator, StringComparison.OrdinalIgnoreCase)
			? path[prefixWithSeparator.Length..]
			: path;
	}
}
