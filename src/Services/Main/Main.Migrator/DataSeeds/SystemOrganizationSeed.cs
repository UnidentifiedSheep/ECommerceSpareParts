using Main.Entities.Auth;
using Main.Entities.Organization;
using Main.Enums.Organization;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using Role = Enums.Role;

namespace Main.Migrator.DataSeeds;

public class SystemOrganizationSeed : ISeed<DContext>
{
    public async Task SeedAsync(DContext context)
    {
        var systemUser = await context.Users.SingleOrDefaultAsync(x =>
            x.UserName == nameof(ServiceSecrets.MainApp) &&
            x.Roles.Any(role => role.RoleName == RoleNames.Normalize(nameof(Role.System))));

        if (systemUser is null)
            throw new InvalidOperationException("MainApp system user was not found.");

        var systemId = systemUser.Id;

        var organization = await context.Organizations
            .Include(x => x.Members)
            .Include(x => x.FinancialProfile)
            .SingleOrDefaultAsync(x => x.Id == systemId);

        if (organization is null)
        {
            organization = Organization.CreateSystem(systemId, systemId);
            await context.Organizations.AddAsync(organization);
        }
        else
        {
            EnsureSystemOrganizationIsValid(organization, systemId);
        }

        if (organization.FinancialProfile is null)
            await context.Set<OrganizationFinancialProfile>()
                .AddAsync(OrganizationFinancialProfile.Create(systemId, decimal.MinValue));
        else if (organization.FinancialProfile.MinAllowedBalance != decimal.MinValue)
            organization.FinancialProfile.SetMinAllowedBalance(decimal.MinValue);

        await context.SaveChangesAsync();
    }

    public int GetPriority() => 2;

    private static void EnsureSystemOrganizationIsValid(
        Organization organization,
        Guid systemId)
    {
        if (organization.Type != OrganizationType.System)
            throw new InvalidOperationException(
                $"Organization '{systemId}' exists but is not a system organization.");

        if (organization.Members.Count != 1 || organization.Members[0].UserId != systemId ||
            organization.Members[0].Role != OrganizationRole.Owner)
            throw new InvalidOperationException(
                $"System organization '{systemId}' must be owned by the MainApp system user.");
    }
}
