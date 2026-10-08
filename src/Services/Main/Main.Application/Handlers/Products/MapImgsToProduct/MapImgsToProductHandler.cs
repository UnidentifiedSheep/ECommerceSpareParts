using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Models.Options.S3;
using Attributes;
using Main.Entities.Product;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Handlers.Products.MapImgsToProduct;

[AutoSave]
[Transactional]
public record MapImgsToProductCommand(
	int ProductId,
	IReadOnlyCollection<ProductImageUpload> Images) : ICommand;

public record ProductImageUpload(string Extension, Func<Stream> OpenReadStream);

public class MapImgsToProductHandler(
	IS3Service s3Storage,
	IUnitOfWork unitOfWork,
	IOptions<S3BucketsOptions> bucketsOptions,
	ILogger<MapImgsToProductHandler> logger) : ICommandHandler<MapImgsToProductCommand>
{
	public async Task<Unit> Handle(MapImgsToProductCommand request, CancellationToken cancellationToken)
	{
		var keys = new HashSet<string>();
		var toAdd = new List<ProductImage>();
		var opt = bucketsOptions.Value.Images;
		try
		{
			foreach (var img in request.Images)
			{
				var model = ProductImage.Create(request.ProductId, img.Extension);
				await using var stream = img.OpenReadStream();
				var key = await s3Storage.UploadFileAsync(
					opt.Name,
					stream,
					model.StorageKey,
					"image/webp");
				keys.Add(key);
				toAdd.Add(model);
			}

			await unitOfWork.AddRangeAsync(toAdd, cancellationToken);
			await unitOfWork.SaveChangesAsync(cancellationToken);
		}
		catch (Exception)
		{
			if (keys.Count != 0)
			{
				var deleteResults = await s3Storage
					.TryDeleteFilesAsync(opt.Name, keys, cancellationToken);

				foreach (var failure in deleteResults.Where(result => !result.IsSuccess))
					logger.LogWarning(
						"Failed to clean up uploaded image {Key} from S3: {ErrorCode}: {ErrorMessage}",
						failure.Key,
						failure.ErrorCode,
						failure.ErrorMessage);
			}
			throw;
		}

		return Unit.Value;
	}
}
