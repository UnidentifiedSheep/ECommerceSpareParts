using System.Text.Json.Serialization;

namespace Main.Application.Orders.Dtos;

public record UpsertOrderDto
{
	[JsonPropertyName("id")]
	public Guid? Id { get; init; }

	[JsonPropertyName("organizationId")]
	public Guid OrganizationId { get; init; }

	[JsonPropertyName("userId")]
	public Guid? UserId { get; init; }

	[JsonPropertyName("currencyId")]
	public required int CurrencyId { get; init; }

	[JsonPropertyName("items")]
	public required IReadOnlyCollection<UpsertOrderItemDto> Items { get; init; }

	[JsonPropertyName("rowVersion")]
	public required uint? RowVersion { get; init; }
}
