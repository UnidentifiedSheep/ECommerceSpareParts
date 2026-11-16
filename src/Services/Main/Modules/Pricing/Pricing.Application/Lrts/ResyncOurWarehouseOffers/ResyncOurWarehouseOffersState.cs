using System.Text.Json.Serialization;
using Application.Common;

namespace Pricing.Application.Lrts.ResyncOurWarehouseOffers;

public record ResyncOurWarehouseOffersState : NoneInputState
{
	[JsonPropertyName("lastStorageContentId")]
	public int LastStorageContentId { get; init; }

	[JsonPropertyName("processedRows")]
	public long ProcessedRows { get; init; }
}
