using System.Text.Json.Serialization;
using Enums;
using Main.Entities.Logistics.Enums;

namespace Main.Modules.Logistics.Application.Dtos;

public record DeliveryScheduleDto
{
	[JsonPropertyName("supplier")]
	public required Supplier Supplier { get; init; }

	[JsonPropertyName("storageCode")]
	public required string StorageCode { get; init; }

	[JsonPropertyName("days")]
	public required DeliveryDays Days { get; init; }

	[JsonPropertyName("exceptions")]
	public required IReadOnlyList<DateOnly> Exceptions { get; init; }
}
