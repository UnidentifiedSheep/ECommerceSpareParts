using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Locan.Core.Interfaces;
using Main.Entities;
using Microsoft.Extensions.Logging;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.DeleteProductImages;

public class DeleteProductImagesLrt(
	IApplicationTransactionFactory transactions,
	IS3Service storage,
	ILogger<DeleteProductImagesLrt> logger) : LrtBase<DeleteProductImagesInputState, NoneInputState>(transactions, logger)
{
	public const string Name = nameof(DeleteProductImagesLrt);
	public override string SystemName => Name;
	public override ILocalizableMessage NameLocalizationMessage => LrtProductImagesDeletionNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage => LrtProductImagesDeletionDescriptionMessage.Instance;

	protected override async Task DoWork(DeleteProductImagesInputState inputState)
	{
		// Deleting the same S3 key again is safe if an earlier attempt was interrupted.
		// S3 returns true for non existing key.
		foreach (var key in inputState.StorageKeys.Distinct(StringComparer.Ordinal))
		{
			var result = await storage.DeleteFileAsync(inputState.BucketName, key, CancellationToken);
			if (!result.IsSuccess)
				throw new InvalidOperationException(
					$"Failed to delete product image '{key}': {result.ErrorCode}: {result.ErrorMessage}");
		}
	}
}
