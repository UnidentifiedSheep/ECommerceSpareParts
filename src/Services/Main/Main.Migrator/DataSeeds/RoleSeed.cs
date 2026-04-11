using Main.Abstractions.Extensions;
using Main.Entities;
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
        var existingRoles = await context.Roles.Select(x => x.NormalizedName).ToHashSetAsync();
        
        var roles = new[]
        {
            new Role
            {
                Name = nameof(RoleEnum.Admin) , 
                NormalizedName = RoleEnum.Admin.ToNormalized(), 
                Description = "Administrator"
            },
            new Role
            {
                Name = nameof(RoleEnum.System), 
                NormalizedName = RoleEnum.System.ToNormalized(), 
                Description = "SYSTEM", 
            },
            new Role
            {
                Name = nameof(RoleEnum.Worker), 
                NormalizedName = RoleEnum.Worker.ToNormalized(), 
                Description = "Worker"
            },
            new Role
            {
                Name = nameof(RoleEnum.Member), 
                NormalizedName = RoleEnum.Member.ToNormalized(), 
                Description = "Member"
            },
            
            new Role
            {
                Name = nameof(RoleEnum.Supplier), 
                NormalizedName = RoleEnum.Supplier.ToNormalized(), 
                Description = "Supplier"
            }
        };
        
        var notExistingRoles = roles
            .Where(x => !existingRoles.Contains(x.NormalizedName))
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