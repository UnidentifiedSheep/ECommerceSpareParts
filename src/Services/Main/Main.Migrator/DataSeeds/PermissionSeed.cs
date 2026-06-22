using Enums;
using Main.Entities.Auth;
using Main.Persistence.Context;
using Persistence.Interfaces;

namespace Main.Migrator.DataSeeds;

public class PermissionSeed : ISeed<DContext>
{
    public async Task SeedAsync(DContext context)
    {
        var permissions = GetPermissions();
        var existingPermissions = context.Permissions
            .Select(p => p.Name)
            .ToHashSet();

        var newPermissions = permissions
            .Where(p => !existingPermissions.Contains(p.Name))
            .ToList();
        if (newPermissions.Count == 0) return;

        await context.Permissions.AddRangeAsync(newPermissions);
        await context.SaveChangesAsync();
    }

    public int GetPriority()
    {
        return 0;
    }

    private Permission[] GetPermissions()
    {
        return Enum.GetValues<PermissionCodes>()
            .Select(x => new Permission(x))
            .ToArray();
    }
}
