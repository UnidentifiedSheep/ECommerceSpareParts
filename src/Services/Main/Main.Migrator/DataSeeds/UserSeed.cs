using Main.Entities.User;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Migrator.DataSeeds;

public class UserSeed : ISeed<DContext>
{
    private const string System = "SYSTEM";
    public async Task SeedAsync(DContext context)
    {
        if (await context.Users.AnyAsync(x => x.NormalizedUserName == System))
            return;
        
        var systemUser = new User
        {
            UserName = System,
            NormalizedUserName = System,
            PasswordHash = ""
        };

        await context.Users.AddAsync(systemUser);
        await context.SaveChangesAsync();
    }

    public int GetPriority()
    {
        return 1;
    }
}