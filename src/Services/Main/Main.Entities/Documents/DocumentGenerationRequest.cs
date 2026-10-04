using System.Linq.Expressions;
using Domain;
using Domain.CommonEntities.Job;
using Domain.Interfaces;
using Domain.Validation;

namespace Main.Entities.Documents;

public sealed class DocumentGenerationRequest
	: Entity<DocumentGenerationRequest, Guid>, ILinqEntity<DocumentGenerationRequest, Guid>
{
	public Guid RequestId { get; private set; }
	public Guid JobId { get; private set; }
	public string DocumentSystemName { get; private set; } = null!;
	public Guid? RequesterId { get; private set; }
	public DateTime CreatedAtUtc { get; private set; }
	public string? BucketName { get; private set; }
	public string? StorageKey { get; private set; }
	public DateTime? GeneratedAtUtc { get; private set; }
	public DateTime? ExpiresAtUtc { get; private set; }

	public Job Job { get; private set; } = null!;

	private DocumentGenerationRequest()
	{
	}

	private DocumentGenerationRequest(
		Guid jobId,
		string documentSystemName,
		Guid? requesterId)
	{
		RequestId = Guid.NewGuid();
		JobId = jobId.EnsureNotEqual(
			Guid.Empty,
			() => new InvalidOperationException("Job ID cannot be empty."));
		DocumentSystemName = documentSystemName
			.EnsureNotNullOrWhiteSpace(() =>
				new InvalidOperationException("Document system name is required."))
			.EnsureMaxLength(128, () =>
				new InvalidOperationException("Document system name is too long."));

		RequesterId = requesterId?.EnsureNotEqual(
			Guid.Empty,
			() => new InvalidOperationException("Requester ID cannot be empty."));
		CreatedAtUtc = DateTime.UtcNow;
	}

	public static DocumentGenerationRequest Create(
		Guid jobId,
		string documentSystemName,
		Guid? requesterId = null)
		=> new(jobId, documentSystemName, requesterId);

	public void Complete(
		string bucketName,
		string storageKey,
		DateTime generatedAtUtc,
		DateTime expiresAtUtc)
	{
		if (StorageKey is not null)
			throw new InvalidOperationException("Document generation is already completed.");

		var validatedBucketName = bucketName
			.EnsureNotNullOrWhiteSpace(() =>
				new InvalidOperationException("Bucket name is required."))
			.EnsureMaxLength(255, () =>
				new InvalidOperationException("Bucket name is too long."));
		var validatedStorageKey = storageKey
			.EnsureNotNullOrWhiteSpace(() =>
				new InvalidOperationException("Storage key is required."))
			.EnsureMaxLength(1024, () =>
				new InvalidOperationException("Storage key is too long."));
		generatedAtUtc.Ensure(
			value => value.Kind == DateTimeKind.Utc,
			() => new InvalidOperationException("Generation time must be UTC."));
		expiresAtUtc.Ensure(
			value => value.Kind == DateTimeKind.Utc && value > generatedAtUtc,
			() => new InvalidOperationException("Expiration time must be UTC and after generation."));

		BucketName = validatedBucketName;
		StorageKey = validatedStorageKey;
		GeneratedAtUtc = generatedAtUtc;
		ExpiresAtUtc = expiresAtUtc;
	}

	public override Guid GetId() => RequestId;

	public static Expression<Func<DocumentGenerationRequest, Guid>> GetKeySelector()
		=> request => request.RequestId;

	public static Expression<Func<DocumentGenerationRequest, bool>> GetEqualityExpression(Guid key)
		=> request => request.RequestId == key;
}
