using Abstractions.Interfaces.Validators;
using Extensions;
using Main.Abstractions.Extensions;
using Main.Entities;
using Main.Entities.Auth;
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

        var adminRole = await context.Roles.FirstAsync(x => x.NormalizedName == Role.Admin.ToNormalized());

        var upperName = AdministratorName.ToUpperInvariant();
        var email = $"{upperName}@example.com";
        var user = new User
        {
            UserName = AdministratorName,
            NormalizedUserName = AdministratorName.ToNormalized(),
            PasswordHash = passwordManager.GetHashOfPassword("SuperSecretPassword.21"),
            TwoFactorEnabled = false,
            AccessFailedCount = 0,
            UserRoles =
            [
                new UserRole
                {
                    AssignedAt = DateTime.UtcNow,
                    RoleName = adminRole.NormalizedName
                }
            ],
            UserInfo = new UserInfo
            {
                Name = AdministratorName,
                Surname = AdministratorName,
                Description = "",
                IsSupplier = false,
                SearchColumn = $"{upperName} {upperName}"
            },
            UserEmails =
            [
                new UserEmail
                {
                    Confirmed = true,
                    ConfirmedAt = DateTime.UtcNow,
                    Email = email,
                    EmailType = EmailType.Personal,
                    IsPrimary = true,
                    NormalizedEmail = email.ToNormalizedEmail(),
                }
            ]
        };
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public int GetPriority() => 1;
}