using Bogus;
using Main.Entities.Auth;
using Main.Entities.Auth.ValueObjects;
using Test.Common.Abstractions;

namespace Tests.DataBuilders.Auth;

public class RoleBuilder(Faker faker) : BuilderBase<Role>(faker)
{
    public RoleName? Name { get; private set; }
    public string? Description { get; private set; }

    public RoleBuilder WithName(RoleName name)
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
        var r = Role.Create(Name ?? Faker.Lorem.Word());
        r.SetDescription(Description);
        return r;
    }
}