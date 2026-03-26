using Bogus;
using Extensions;
using Main.Entities;

namespace Tests.MockData.DataFactories;

public static class UserFactory
{
    private static readonly Faker<User> Faker = new Faker<User>(Global.Locale)
        .RuleFor(x => x.UserName, f => f.Person.UserName)
        .RuleFor(x => x.PasswordHash, f => f.Random.Words(10))
        .RuleFor(x => x.TwoFactorEnabled, f => f.Random.Bool())
        .RuleFor(x => x.UserInfo, f => new UserInfo
        {
            Name = f.Person.FirstName,
            Surname = f.Person.LastName,
            Description = f.Person.Address.Street,
            IsSupplier = f.Random.Bool(),
            SearchColumn = $"{f.Person.FirstName} {f.Person.LastName} {f.Person.UserName} {f.Person.Address.Street}"
        })
        .FinishWith((_, x) => { x.NormalizedUserName = x.UserName.ToNormalized(); });

    public static List<User> Create(int count)
    {
        return Faker.Generate(count);
    }

    public static User Create()
    {
        return Create(1)[0];
    }
}