using Abstractions.Interfaces.Validators;
using Main.Entities.Auth;
using Main.Entities.User;
using Main.Enums;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence.Interfaces;
using Role = Main.Enums.Role;

namespace Main.Migrator.DataSeeds;

public class UserSeed(
    IOptions<ServiceSecrets> secrets,
    IPasswordManager pwdManager) : ISeed<DContext>
{
    private static readonly string[] Services =
    [
        nameof(ServiceSecrets.MainApp),
        nameof(ServiceSecrets.Analytics),
        nameof(ServiceSecrets.Pricing),
        nameof(ServiceSecrets.Search)
    ];

    private readonly ServiceSecrets _secrets = secrets.Value;

    public async Task SeedAsync(DContext context)
    {
        var existingSystemUsers = await context.Users
            .Where(x => x.Roles.Any(y => y.RoleName == RoleNames.Normalize(nameof(Role.System))))
            .AsNoTracking()
            .ToDictionaryAsync(x => x.UserName);

        var toAdd = new List<User>();

        foreach (var service in Services)
        {
            if (existingSystemUsers.ContainsKey(service))
                continue;

            var secret = GetServiceSecret(service);
            toAdd.Add(CreateSystemUser(service, secret));
        }

        if (toAdd.Count == 0)
            return;

        await context.Users.AddRangeAsync(toAdd);
        await context.SaveChangesAsync();
    }

    public int GetPriority()
    {
        return 1;
    }

    private string GetServiceSecret(string service)
    {
        return service switch
        {
            nameof(ServiceSecrets.MainApp) => _secrets.MainApp,
            nameof(ServiceSecrets.Analytics) => _secrets.Analytics,
            nameof(ServiceSecrets.Pricing) => _secrets.Pricing,
            nameof(ServiceSecrets.Search) => _secrets.Search,
            _ => throw new InvalidOperationException($"Unknown service: {service}")
        };
    }

    private User CreateSystemUser(string service, string secret)
    {
        var systemUser = User.Create(service, pwdManager.GetHashOfPassword(secret));
        systemUser.AddRole(nameof(Role.System));

        return systemUser;
    }
}
