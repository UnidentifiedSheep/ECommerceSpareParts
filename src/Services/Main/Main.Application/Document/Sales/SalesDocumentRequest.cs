using System.Text.Json.Serialization;
using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;

namespace Main.Application.Document.Sales;

public record SalesDocumentRequest : IDocumentRequest
{
	[JsonPropertyName("documentType")]
	public required DocumentType DocumentType { get; init; }
}
