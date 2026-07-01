using Bogus;
using Enums;
using Main.Enums;

namespace Tests.DataBuilders.User;

public class AdminUserBuilder(Faker faker) : UserBuilder(faker)
{
    public override Main.Entities.User.User Build()
    {
        WithUserInfo();
        var user = base.Build();
        user.AddRole(nameof(Role.Admin));
        return user;
    }
}