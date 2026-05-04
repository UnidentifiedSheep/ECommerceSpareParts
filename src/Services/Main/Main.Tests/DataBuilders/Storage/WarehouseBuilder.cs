using Bogus;
using Main.Enums;

namespace Tests.DataBuilders.Storage;

public class WarehouseBuilder(Faker faker) : StorageBuilder(faker)
{
    public override Main.Entities.Storage.Storage Build()
    {
        WithType(StorageType.Warehouse);
        return base.Build();
    }
}