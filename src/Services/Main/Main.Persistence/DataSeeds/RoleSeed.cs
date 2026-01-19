using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Persistence.DataSeeds;

public class RoleSeed : ISeed<DContext>
{
    public async Task SeedAsync(DContext context)
    {
        var existingRoles = await context.Roles.Select(x => x.NormalizedName).ToHashSetAsync();
        var roles = new[]
        {
            new Role { Name = "Admin", NormalizedName = "ADMIN", Description = "Administrator"},
            new Role { Name = "System", NormalizedName = "SYSTEM", Description = "SYSTEM", IsSystem = true},
            new Role { Name = "Worker", NormalizedName = "WORKER", Description = "Worker"},
            new Role { Name = "Member", NormalizedName = "MEMBER", Description = "Member"},
        };
        var notExistingRoles = roles
            .Where(x => !existingRoles.Contains(x.NormalizedName))
            .ToList();
        if (notExistingRoles.Count == 0) return;
        
        await context.Roles.AddRangeAsync(notExistingRoles);
        await context.SaveChangesAsync();
    }

    public int GetPriority() => 0;
}