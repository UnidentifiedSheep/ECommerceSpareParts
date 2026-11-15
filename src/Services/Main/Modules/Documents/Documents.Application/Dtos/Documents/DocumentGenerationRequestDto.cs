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

	[JsonPropertyName("createdAt")]
	public required DateTime CreatedAt { get; init; }

	[JsonPropertyName("generatedAt")]
	public DateTime? GeneratedAt { get; init; }

	[JsonPropertyName("expiresAt")]
	public DateTime? ExpiresAt { get; init; }

	[JsonPropertyName("bucketName")]
	public string? BucketName { get; init; }

	[JsonPropertyName("storageKey")]
	public string? StorageKey { get; init; }
}
