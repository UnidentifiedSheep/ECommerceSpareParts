using System.Text.Json.Serialization;
using Main.Enums.Orders;

namespace Main.Application.Orders.Dtos;

public sealed record OrderDto
{
	[JsonPropertyName("id")]
	public required Guid Id { get; init; }

	[JsonPropertyName("organizationId")]
	public required Guid OrganizationId { get; init; }

	[JsonPropertyName("userId")]
	public required Guid? UserId { get; init; }

	[JsonPropertyName("currencyId")]
	public required int CurrencyId { get; init; }

	[JsonPropertyName("source")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public required OrderSource Source { get; init; }

	[JsonPropertyName("status")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public required OrderStatus Status { get; init; }

	[JsonPropertyName("fulfillmentStatus")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public required OrderFulfillmentStatus FulfillmentStatus { get; init; }

	[JsonPropertyName("confirmedByUserId")]
	public required Guid? ConfirmedByUserId { get; init; }

	[JsonPropertyName("confirmedAt")]
	public required DateTime? ConfirmedAt { get; init; }

	[JsonPropertyName("createdAt")]
	public required DateTime CreatedAt { get; init; }

	[JsonPropertyName("updatedAt")]
	public required DateTime UpdatedAt { get; init; }

	[JsonPropertyName("rowVersion")]
	public required uint RowVersion { get; init; }
}
