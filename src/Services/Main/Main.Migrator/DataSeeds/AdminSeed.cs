using Main.Entities.Auth;
using Main.Entities.Organization;
using Main.Entities.User;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Interfaces;
using Security.Core.Interfaces;
using Role = Enums.Role;

namespace Main.Migrator.DataSeeds;

public class AdminSeed(IPasswordManager passwordManager) : ISeed<UnifiedDbContext>
{
	private const string AdministratorName = "Administrator";

	public async Task SeedAsync(UnifiedDbContext context)
	{
		var user = await context.Set<User>().SingleOrDefaultAsync(x => x.UserName == AdministratorName);

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
			user.AddEmail(
				email,
				EmailType.Personal,
				true,
				true);

			await context.Set<User>().AddAsync(user);
		}

		if (!await context.Set<Organization>().AnyAsync(x => x.Id == user.Id))
			await context.Set<Organization>()
				.AddAsync(Organization.CreateIndividual(AdministratorName, user.Id));

		await context.SaveChangesAsync();
	}

	public int ExecutionOrder => 1;
}
