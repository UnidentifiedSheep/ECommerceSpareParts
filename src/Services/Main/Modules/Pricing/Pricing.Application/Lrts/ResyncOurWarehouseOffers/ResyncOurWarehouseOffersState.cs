using System.Text.Json.Serialization;
using Application.Common;
using Application.Common.LRT;

namespace Pricing.Application.Lrts.ResyncOurWarehouseOffers;

public record ResyncOurWarehouseOffersState : NoneInputState
{
	[JsonPropertyName("lastStorageContentId")]
	public int LastStorageContentId { get; init; }

	[JsonPropertyName("processedRows")]
	public long ProcessedRows { get; init; }
}
