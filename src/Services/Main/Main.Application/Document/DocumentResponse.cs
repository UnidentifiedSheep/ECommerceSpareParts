using System.Text.Json.Serialization;
using Main.Application.Interfaces.Services.Document;

namespace Main.Application.Document;

public record DocumentResponse : IDocumentResponse
{
	[JsonPropertyName("bucketName")]
	public required string BucketName { get; init; }

	[JsonPropertyName("storageKey")]
	public required string StorageKey { get; init; }
}
