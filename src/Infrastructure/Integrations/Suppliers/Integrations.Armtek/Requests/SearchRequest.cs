using Enums;

namespace Integrations.Armtek.Requests;

public record SearchRequest
{
	public required string Number { get; init; }

	public string? Brand { get; init; }

	public bool WithAnalogues { get; init; }

	public required string PointCode { get; init; }

	public required DeliveryMode DeliveryMode { get; init; }
}
