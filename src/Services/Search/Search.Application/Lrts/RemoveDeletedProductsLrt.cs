using Application.Common.Interfaces.Lrt;
using Application.Common.LRT;
using Locan.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Search.Abstractions;
using Search.Abstractions.Options;
using Search.Application.Interfaces.Product;

namespace Search.Application.Lrts;

public sealed class RemoveDeletedProductsLrt(
	IProductRepository repository,
	IOptionsMonitor<DeletedProductsCleanupOptions> options,
	TimeProvider timeProvider,
	ILogger<RemoveDeletedProductsLrt> logger) : ILrtNamedObject<NoneInputState>
{
	public const string Name = nameof(RemoveDeletedProductsLrt);

	public string SystemName => Name;
	public Type InputType => typeof(NoneInputState);
	public Type StateType => typeof(NoneInputState);
	public ILocalizableMessage NameLocalizationMessage => LrtDeletedProductsRemovalNameMessage.Instance;
	public ILocalizableMessage DescriptionLocalizationMessage => LrtDeletedProductsRemovalDescriptionMessage.Instance;

	public async Task ExecuteAsync(Guid jobId, Guid leaseHolderId, CancellationToken cancellationToken = default)
	{
		var settings = options.CurrentValue;
		var deletedBefore = timeProvider.GetUtcNow().UtcDateTime.AddDays(-settings.RetentionDays);
		long totalDeleted = 0;
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var deleted = await repository.DeleteDeletedBefore(deletedBefore, settings.BatchSize, cancellationToken);
			totalDeleted += deleted;
			if (deleted == 0) break;
		}

		logger.LogInformation(
			"Removed {DeletedCount} product documents marked as deleted at or before {DeletedBefore}.",
			totalDeleted,
			deletedBefore);
	}
}
