using System.Text.Json.Serialization;
using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;

namespace Main.Application.Document;

public abstract record DocumentRequestBase : IDocumentRequest
{
	[JsonPropertyName("documentType")]
	public required DocumentType DocumentType { get; init; }
}
