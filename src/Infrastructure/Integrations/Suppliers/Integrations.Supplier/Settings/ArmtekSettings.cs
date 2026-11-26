namespace Integrations.Supplier.Settings;

public class ArmtekSettings
{
	public IReadOnlyList<ArmtekStoragePoint> Storages { get; init; } = [];

	public ArmtekStoragePoint? FindStorage(string storageCode)
		=> Storages.FirstOrDefault(x => string.Equals(
			x.StorageCode.Trim(),
			storageCode.Trim(),
			StringComparison.OrdinalIgnoreCase));
}
