using Bogus;
using Main.Entities.Auth.ValueObjects;
using Main.Entities.User.ValueObjects;
using Test.Common.Abstractions;

namespace Tests.DataBuilders.User;

public class UserBuilder(Faker faker) : BuilderBase<Main.Entities.User.User>(faker)
{
    public UserName? UserName { get; private set; }
    public string? PasswordHash { get; private set; }
    
    private readonly List<RoleName> _roles = [];
    public IReadOnlyList<RoleName> Roles => _roles;

    public UserBuilder WithUserName(UserName userName)
    {
        UserName = userName;
        return this;
    }

    public UserBuilder WithPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        return this;
    }

    public UserBuilder WithRole(RoleName role)
    {
        _roles.Add(role);
        return this;
    }

    public override Main.Entities.User.User Build()
    {
        var user =  Main.Entities.User.User.Create(
            UserName ?? Faker.Internet.UserName(), 
            PasswordHash ?? Faker.Lorem.Letter(24));

        foreach (var role in Roles)
            user.AddRole(role);
        
        return user;
    }
}