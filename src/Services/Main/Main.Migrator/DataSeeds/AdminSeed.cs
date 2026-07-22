using Abstractions.Interfaces.Validators;
using Main.Entities.Auth;
using Main.Entities.Organization;
using Main.Entities.User;
using Main.Enums;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using Role = Enums.Role;

namespace Main.Migrator.DataSeeds;

public class AdminSeed(IPasswordManager passwordManager) : ISeed<DContext>
{
    private const string AdministratorName = "Administrator";

    public async Task SeedAsync(DContext context)
    {
        var user = await context.Users
            .SingleOrDefaultAsync(x => x.UserName == AdministratorName);

        if (user is null)
        {
            var upperName = AdministratorName.ToUpperInvariant();
            var email = $"{upperName}@example.com";

            user = User.Create(
                AdministratorName,
                passwordManager.GetHashOfPassword("SuperSecretPassword.21"));
            user.AddRole(RoleNames.Normalize(nameof(Role.Admin)));
            user.SetUserInfo(
                AdministratorName,
                AdministratorName,
                null);
            user.AddUserEmail(
                email,
                EmailType.Personal,
                true,
                true);

            await context.Users.AddAsync(user);
        }

        if (!await context.Organizations.AnyAsync(x => x.Id == user.Id))
            await context.Organizations.AddAsync(
                Organization.CreateIndividual(AdministratorName, user.Id));

        await context.SaveChangesAsync();
    }

    public int GetPriority() { return 1; }
}
