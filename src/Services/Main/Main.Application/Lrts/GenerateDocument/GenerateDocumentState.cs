using System.Text.Json.Serialization;
using Application.Common.Interfaces.Lrt;

namespace Main.Application.Lrts.GenerateDocument;

public record GenerateDocumentState
{
	[JsonPropertyName("generatedFileLink")]
	public string? GeneratedFileLink { get; init; }

	[JsonPropertyName("bucketName")]
	public string? BucketName { get; init; }

	[JsonPropertyName("storageKey")]
	public string? StorageKey { get; init; }

	[JsonPropertyName("generatedAtUtc")]
	public DateTime? GeneratedAtUtc { get; init; }

	[JsonPropertyName("notificationProcessed")]
	public bool NotificationProcessed { get; init; }
}

public record GenerateDocumentInputState : IInputState
{
	[JsonPropertyName("documentSystemName")]
	public required string DocumentSystemName { get; init; }

	[JsonPropertyName("documentRequest")]
	public required string DocumentRequest { get; init; }

	public void ValidateState()
	{
		if (string.IsNullOrWhiteSpace(DocumentSystemName))
			throw new InvalidOperationException("Document system name is required.");

		if (string.IsNullOrWhiteSpace(DocumentRequest))
			throw new InvalidOperationException("Document request is required.");
	}
}
