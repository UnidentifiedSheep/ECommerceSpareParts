using Persistence.Context;
using Security.Core.Interfaces;
using Application.Common.Interfaces.Validators;
using Main.Entities.Auth;
using Main.Entities.User;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence.Interfaces;
using Role = Enums.Role;

namespace Main.Migrator.DataSeeds;

public class UserSeed(IOptions<ServiceSecrets> secrets, IPasswordManager pwdManager) : ISeed<UnifiedDbContext>
{
	private static readonly string[] Services =
	[
		nameof(ServiceSecrets.MainApp),
		nameof(ServiceSecrets.Pricing),
		nameof(ServiceSecrets.Search),
		nameof(ServiceSecrets.Gateway)
	];

	private readonly ServiceSecrets _secrets = secrets.Value;

	public async Task SeedAsync(UnifiedDbContext context)
	{
		var existingSystemUsers = await context
			.Set<User>()
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

		await context.Set<User>().AddRangeAsync(toAdd);
		await context.SaveChangesAsync();
	}

	public int ExecutionOrder => 1;

	private string GetServiceSecret(string service)
	{
		return service switch
		{
			nameof(ServiceSecrets.MainApp) => _secrets.MainApp,
			nameof(ServiceSecrets.Pricing) => _secrets.Pricing,
			nameof(ServiceSecrets.Search) => _secrets.Search,
			nameof(ServiceSecrets.Gateway) => _secrets.Gateway,
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
