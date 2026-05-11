using Bogus;
using Main.Enums;

namespace Tests.DataBuilders.Storage;

public class SupplierStorageBuilder(Faker faker) : StorageBuilder(faker)
{
    public override Main.Entities.Storage.Storage Build()
    {
        WithType(StorageType.SupplierStorage);
        return base.Build();
    }
}