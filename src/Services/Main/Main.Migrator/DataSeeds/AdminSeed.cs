using Abstractions.Interfaces.Validators;
using Main.Entities.Auth.ValueObjects;
using Main.Entities.User;
using Main.Enums;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using Role = Main.Enums.Role;

namespace Main.Migrator.DataSeeds;

public class AdminSeed(IPasswordManager passwordManager) : ISeed<DContext>
{
    private const string AdministratorName = "Administrator";
    public async Task SeedAsync(DContext context)
    {
        if (await context.Users.AnyAsync(x => x.UserName == AdministratorName))
            return;

        var upperName = AdministratorName.ToUpperInvariant();
        var email = $"{upperName}@example.com";

        var user = User.Create(AdministratorName, passwordManager.GetHashOfPassword("SuperSecretPassword.21"));
        user.AddUserRole(RoleName.ToNormalized(nameof(Role.Admin)));
        user.SetUserInfo(AdministratorName, AdministratorName, null);
        user.AddUserEmail(email, EmailType.Personal, true, true);
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public int GetPriority() => 1;
}