using System.Text.Json.Serialization;
using Application.Common;

namespace Main.Modules.Product.Application.Lrts.EnrichCatalogue.MapCatalogueCandidatesToProducts;

public sealed record MapCatalogueCandidatesToProductsState : NoneInputState
{
	[JsonPropertyName("lastProcessedId")]
	public Guid LastProcessedId { get; init; }

	[JsonPropertyName("processedRows")]
	public long ProcessedRows { get; init; }

	[JsonPropertyName("mappedRows")]
	public long MappedRows { get; init; }

	[JsonPropertyName("skippedRows")]
	public long SkippedRows { get; init; }
}
