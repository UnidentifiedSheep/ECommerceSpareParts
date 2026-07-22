using Main.Entities.Organization;
using Main.Entities.User;
using Main.Persistence.Context;
using Tests.DataBuilders.User;

namespace Tests.Extensions;

public static class UserBuilderExtensions
{
    public static async Task<User> BuildAndAddToDb(
        this UserBuilder builder,
        DContext context)
    {
        var user = builder.Build();
        var organization = CreateIndividualOrganization(user);

        await context.AddRangeAsync(user, organization);
        await context.SaveChangesAsync();
        return user;
    }

    public static async Task<IReadOnlyCollection<User>> BuildManyAndAddToDb(
        this UserBuilder builder,
        DContext context,
        int count)
    {
        var users = builder.BuildMany(count).ToList();
        var organizations = users.Select(CreateIndividualOrganization).ToList();

        await context.AddRangeAsync(users);
        await context.AddRangeAsync(organizations);
        await context.SaveChangesAsync();
        return users;
    }

    private static Organization CreateIndividualOrganization(User user)
    {
        var name = user.UserInfo is null
            ? user.UserName.Value
            : $"{user.UserInfo.Name} {user.UserInfo.Surname}";

        return Organization.CreateIndividual(name, user.Id);
    }
}
