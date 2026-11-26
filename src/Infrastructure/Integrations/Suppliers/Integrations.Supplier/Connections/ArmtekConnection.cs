using Enums;

namespace Integrations.Supplier.Connections;

public record ArmtekConnection
{
	public required string BaseUrl { get; init; }

	public required string Login { get; init; }

	public required string Password { get; init; }

	public required string SalesOrganization { get; init; }

	public required string CustomerCode { get; init; }

	public string? ContractCode { get; init; }

	public IReadOnlyList<ArmtekStorageConnection> Storages { get; init; } = [];

	public ArmtekStorageConnection? FindStorage(string storageCode)
		=> Storages.FirstOrDefault(x => string.Equals(
			x.StorageCode.Trim(),
			storageCode.Trim(),
			StringComparison.OrdinalIgnoreCase));
}

public record ArmtekStorageConnection
{
	public required string StorageCode { get; init; }

	public required DeliveryMode DeliveryMode { get; init; }

	public required string PointCode { get; init; }
}
