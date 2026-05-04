using Bogus;
using Main.Enums;

namespace Tests.DataBuilders.User;

public class MemberUserBuilder(Faker faker) : UserBuilder(faker)
{
    public override Main.Entities.User.User Build()
    {
        WithUserInfo();
        var user = base.Build();
        user.AddRole(nameof(Role.Member));
        return user;
    }
}