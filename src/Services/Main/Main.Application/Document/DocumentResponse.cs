using System.Text.Json.Serialization;
using Main.Application.Interfaces.Services.Document;

namespace Main.Application.Document;

public record DocumentResponse : IDocumentResponse
{
	[JsonPropertyName("generatedFileLink")]
	public required string GeneratedFileLink { get; init; }
}
