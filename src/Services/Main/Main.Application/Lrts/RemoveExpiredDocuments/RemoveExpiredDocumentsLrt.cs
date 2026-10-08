using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Attributes;
using Domain.CommonEntities.Job;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Documents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using S3.Core.Interfaces;

namespace Main.Application.Lrts.RemoveExpiredDocuments;

public class RemoveExpiredDocumentsLrt(
	IRepository<Job, Guid> jobRepository,
	IUnitOfWork unitOfWork,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	IReadRepository<DocumentGenerationRequest, Guid> documentRepository,
	IS3Service s3Service,
	ILogger<RemoveExpiredDocumentsLrt> logger) : LrtBase<NoneInputState, RemoveExpiredDocumentsState>(
	jobRepository,
	unitOfWork,
	publisher,
	transactionService,
	logger)
{
	public const string Name = nameof(RemoveExpiredDocumentsLrt);
	public override string SystemName => Name;

	public override ILocalizableMessage NameLocalizationMessage =>
		LrtExpiredDocumentsRemovalNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage =>
		LrtExpiredDocumentsRemovalDescriptionMessage.Instance;

	protected override async Task DoWork(NoneInputState inputState)
	{
		const int batchSize = 1000;
		var expiredBefore = DateTime.UtcNow;

		while (true)
		{
			var lastProcessedId = State.LastProcessedId;
			var batch = await documentRepository.Query
				.Where(request => request.BucketName != null && request.StorageKey != null)
				.Where(request => request.ExpiresAt <= expiredBefore)
				.Where(request => lastProcessedId == null || request.RequestId > lastProcessedId)
				.OrderBy(request => request.RequestId)
				.Select(request => new ExpiredDocument(
					request.RequestId,
					request.BucketName!,
					request.StorageKey!))
				.Take(batchSize)
				.ToListAsync(CancellationToken);

			if (batch.Count == 0) return;

			var (deletedIds, failedCount) = await DeleteFilesAsync(batch);

			if (deletedIds.Count != 0)
				await TransactionService.ExecuteAsync(
					TransactionalAttribute.ReadCommitted(30, 3),
					async (context, ct) =>
						await context.Repositories.Get<DocumentGenerationRequest, Guid>()
							.DeleteManyAsync(deletedIds, ct),
					CancellationToken);

			if (failedCount != 0)
				throw new InvalidOperationException(
					$"Failed to remove {failedCount} expired document files from S3.");

			await SaveStateAsync(new RemoveExpiredDocumentsState
			{
				LastProcessedId = batch[^1].Id
			});
		}
	}

	private async Task<(List<Guid> DeletedIds, int FailedCount)> DeleteFilesAsync(
		IReadOnlyList<ExpiredDocument> batch)
	{
		var deletedIds = new List<Guid>(batch.Count);
		var failedCount = 0;

		foreach (var group in batch.GroupBy(item => item.Bucket))
		{
			var results = await s3Service.TryDeleteFilesAsync(
				group.Key,
				group.Select(item => item.Key),
				CancellationToken);
			var resultsByKey = results.ToLookup(result => result.Key, StringComparer.Ordinal);

			foreach (var item in group)
			{
				var keyResults = resultsByKey[item.Key].ToArray();
				if (keyResults.Length == 0)
					throw new InvalidOperationException(
						$"S3 did not return a deletion result for '{item.Key}' in bucket '{item.Bucket}'.");

				if (keyResults.All(result => result.IsSuccess))
				{
					deletedIds.Add(item.Id);
					continue;
				}

				failedCount++;
				var error = keyResults.First(result => !result.IsSuccess);
				Logger.LogWarning(
					"Failed to remove expired document {RequestId} from S3 bucket {Bucket}, key {Key}: {ErrorCode}: {ErrorMessage}",
					item.Id,
					item.Bucket,
					item.Key,
					error.ErrorCode,
					error.ErrorMessage);
			}
		}

		return (deletedIds, failedCount);
	}

	private sealed record ExpiredDocument(Guid Id, string Bucket, string Key);
}
