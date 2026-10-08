using System.Text.Json.Serialization;

namespace Main.Application.Lrts.RemoveExpiredDocuments;

public record RemoveExpiredDocumentsState
{
	[JsonPropertyName("lastProcessedId")]
	public Guid? LastProcessedId { get; init; }
}
