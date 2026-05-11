using Bogus;
using Main.Enums;
using Test.Common.Abstractions;

namespace Tests.DataBuilders.Storage;

public class StorageBuilder(Faker faker) : BuilderBase<Main.Entities.Storage.Storage>(faker)
{
    public string? Name { get; private set; }
    public StorageType? Type { get; private set; }
    public string? Location { get; private set; }
    public string? Description { get; private set; }

    public StorageBuilder WithName(string name)
    {
        Name = name;
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
            Name ?? Faker.Lorem.Letter(7),
            Type ?? Faker.PickRandom<StorageType>());

        storage.SetDescription(Description);
        storage.SetLocation(Location);

        return storage;
    }
}