using Bogus;
using Main.Enums;
using Tests.Abstractions;

namespace Tests.DataBuilders.Storage;

public class StorageBuilder(Faker faker) : BuilderBase<Main.Entities.Storage.Storage>(faker)
{
    public string? Code { get; private set; }
    public StorageType? Type { get; private set; }
    public string? Location { get; private set; }
    public string? Description { get; private set; }

    public StorageBuilder WithCode(string code)
    {
        Code = code;
        return this;
    }

    public StorageBuilder WithType(StorageType type)
    {
        Type = type;
        return this;
    }

    public StorageBuilder WithLocation(string location)
    {
        Location = location;
        return this;
    }

    public StorageBuilder WithDescription(string description)
    {
        Description = description;
        return this;
    }

    public override Main.Entities.Storage.Storage Build()
    {
        var storage = Main.Entities.Storage.Storage.Create(
            Code ?? Faker.Lorem.Letter(7),
            Type ?? Faker.PickRandom<StorageType>());

        storage.SetDescription(Description);
        storage.SetLocation(Location);

        return storage;
    }
}
