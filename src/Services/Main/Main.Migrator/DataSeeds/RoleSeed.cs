using Main.Entities.Auth;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

using RoleEnum = Main.Enums.Role;

namespace Main.Migrator.DataSeeds;

public class RoleSeed : ISeed<DContext>
{
    public async Task SeedAsync(DContext context)
    {
        var existingRoles = await context.Roles
            .Select(x => x.Name.NormalizedValue)
            .ToHashSetAsync();
        
        var roles = new[]
        {
            Role.Create(nameof(RoleEnum.Admin)),
            Role.Create(nameof(RoleEnum.System)),
            Role.Create(nameof(RoleEnum.Worker)),
            Role.Create(nameof(RoleEnum.Member)),
            Role.Create(nameof(RoleEnum.Supplier))
        };
        
        var notExistingRoles = roles
            .Where(x => !existingRoles.Contains(x.Name.NormalizedValue))
            .ToList();
        if (notExistingRoles.Count == 0) return;

        await context.Roles.AddRangeAsync(notExistingRoles);
        await context.SaveChangesAsync();
    }

    public int GetPriority()
    {
        return 0;
    }
}