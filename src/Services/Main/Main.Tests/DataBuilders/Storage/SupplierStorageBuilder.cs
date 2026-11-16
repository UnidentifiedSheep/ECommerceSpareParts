using Bogus;
using Main.Entities.Storage.Enums;

namespace Tests.DataBuilders.Storage;

public class SupplierStorageBuilder(Faker faker) : StorageBuilder(faker)
{
	public override Main.Entities.Storage.Storage.Storage Build()
	{
		WithType(StorageType.SupplierStorage);
		return base.Build();
	}
}
