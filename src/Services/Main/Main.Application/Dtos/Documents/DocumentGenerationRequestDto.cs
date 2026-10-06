using System.Text.Json.Serialization;
using Domain.CommonEnums;

namespace Main.Application.Dtos.Documents;

public record DocumentGenerationRequestDto
{
	[JsonPropertyName("requestId")]
	public required Guid RequestId { get; init; }

	[JsonPropertyName("jobId")]
	public required Guid JobId { get; init; }

	[JsonPropertyName("status")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public required JobStatus Status { get; init; }

	[JsonPropertyName("documentSystemName")]
	public required string DocumentSystemName { get; init; }

	[JsonPropertyName("requesterId")]
	public Guid? RequesterId { get; init; }

	[JsonPropertyName("createdAtUtc")]
	public required DateTime CreatedAtUtc { get; init; }

	[JsonPropertyName("generatedAtUtc")]
	public DateTime? GeneratedAtUtc { get; init; }

	[JsonPropertyName("expiresAtUtc")]
	public DateTime? ExpiresAtUtc { get; init; }

	[JsonPropertyName("bucketName")]
	public string? BucketName { get; init; }

	[JsonPropertyName("storageKey")]
	public string? StorageKey { get; init; }
}
