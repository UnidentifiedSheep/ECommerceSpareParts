using Main.Core.Entities;
using Main.Persistence.Context;
using Persistence.Interfaces;

namespace Main.Persistence.DataSeeds;

public class UserSeed : ISeed<DContext>
{
    public async Task SeedAsync(DContext context)
    {
        var systemUser = new User
        {
            UserName = "SYSTEM",
            NormalizedUserName = "SYSTEM",
            PasswordHash = ""
        };
        
        await context.Users.AddAsync(systemUser);
        await context.SaveChangesAsync();
    }

    public int GetPriority() => 1;
}