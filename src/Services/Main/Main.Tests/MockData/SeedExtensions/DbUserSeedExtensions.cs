using Main.Entities;
using Main.Persistence.Context;
using Tests.MockData.DataFactories;

namespace Tests.MockData.SeedExtensions;

public static class DbUserSeedExtensions
{
    public static async Task<List<User>> CreateUsers(this DContext ctx, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var users = UserFactory.Create(count);
        await ctx.AddRangeAsync(users);
        await ctx.SaveChangesAsync();
        return users;
    }

    public static async Task<User> CreateSystemUser(this DContext ctx)
    {
        var user = UserFactory.Create(1)[0];
        user.UserName = user.NormalizedUserName = "SYSTEM";
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();
        return user;
    }
}