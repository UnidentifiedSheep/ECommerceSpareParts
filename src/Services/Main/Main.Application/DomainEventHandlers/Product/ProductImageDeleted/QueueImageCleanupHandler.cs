using Application.Common.Abstractions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Services;
using Application.Common.Models.Options.S3;
using Application.Common.Services.Events;
using Main.Application.Lrts.DeleteProductImages;
using Main.Entities.DomainEvents.Product;
using Microsoft.Extensions.Options;

namespace Main.Application.DomainEventHandlers.Product.ProductImageDeleted;

public class QueueImageCleanupHandler(
	IJobService jobService,
	IJobProvider<DeleteProductImagesLrt, DeleteProductImagesInputState> cleanupJobs,
	IOptions<S3BucketsOptions> buckets) : BatchableDomainEventHandler<ProductImageDeletedDomainEvent>
{
	public override async Task Handle(
		Batch<ProductImageDeletedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var jobs = notification.Items
			.GroupBy(image => image.ProductId, image => image.StorageKey)
			.Select(group => cleanupJobs.Create(new DeleteProductImagesInputState
			{
				ProductId = group.Key,
				BucketName = buckets.Value.Images.Name,
				StorageKeys = group.Distinct(StringComparer.Ordinal).ToArray()
			}))
			.ToList();
		if (jobs.Count == 0) return;

		var queued = await jobService.TryEnqueueJobsAsync(jobs, cancellationToken);
		if (queued.Count != jobs.Count)
			throw new InvalidOperationException("Unable to enqueue some product image cleanup jobs.");
	}
}
