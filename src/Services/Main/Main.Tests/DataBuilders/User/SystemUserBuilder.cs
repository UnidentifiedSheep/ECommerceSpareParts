using Bogus;
using Main.Enums;

namespace Tests.DataBuilders.User;

public class SystemUserBuilder(Faker faker) : UserBuilder(faker)
{
    public override Main.Entities.User.User Build()
    {
        WithPasswordHash("");
        var user = base.Build();
        user.AddRole(nameof(Role.System));
        return user;
    }
}