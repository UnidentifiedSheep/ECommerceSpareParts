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
        var systemUserIds = await context.Users
            .Where(x => x.Roles.Any(role =>
                role.RoleName == RoleNames.Normalize(nameof(Role.System))))
            .Select(x => x.Id)
            .ToListAsync();

        if (systemUserIds.Count == 0)
            throw new InvalidOperationException("System users were not found.");

        var systemUserIdSet = systemUserIds.ToHashSet();
        var organizations = await context.Organizations
            .Where(x => systemUserIdSet.Contains(x.Id))
            .Include(x => x.Members)
            .Include(x => x.FinancialProfile)
            .ToDictionaryAsync(x => x.Id);

        foreach (var systemUserId in systemUserIds)
        {
            if (!organizations.TryGetValue(systemUserId, out var organization))
            {
                organization = Organization.CreateSystem(systemUserId, systemUserId);
                await context.Organizations.AddAsync(organization);
            }
            else
            {
                EnsureSystemOrganizationIsValid(organization, systemUserId);
            }

            if (organization.FinancialProfile is null)
                await context.Set<OrganizationFinancialProfile>()
                    .AddAsync(OrganizationFinancialProfile.Create(systemUserId, decimal.MinValue));
            else if (organization.FinancialProfile.MinAllowedBalance != decimal.MinValue)
                organization.FinancialProfile.SetMinAllowedBalance(decimal.MinValue);
        }

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
                $"System organization '{systemId}' must be owned by its system user.");
    }
}
