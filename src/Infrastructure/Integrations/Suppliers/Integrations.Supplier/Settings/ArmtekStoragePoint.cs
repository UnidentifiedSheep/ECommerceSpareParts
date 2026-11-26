using Enums;

namespace Integrations.Supplier.Settings;

public record ArmtekStoragePoint
{
	public required string StorageCode { get; init; }

	public required DeliveryMode DeliveryMode { get; init; }

	public required string PointCode { get; init; }
}
