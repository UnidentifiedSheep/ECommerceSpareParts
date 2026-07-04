using Bogus;
using Main.Entities.Auth;
using Tests.Abstractions;

namespace Tests.DataBuilders.Auth;

public class RoleBuilder(Faker faker) : BuilderBase<Role>(faker)
{
    public string? Name { get; private set; }
    public string? Description { get; private set; }

    public RoleBuilder WithName(string name)
    {
        Name = name;
        return this;
    }

    public RoleBuilder WithDescription(string? description)
    {
        Description = description;
        return this;
    }

    public override Role Build()
    {
        var r = Role.Create(Name ?? Faker.Lorem.Letter(5));
        r.SetDescription(Description);
        return r;
    }
}