using Enums;
using Main.Entities.Auth;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Migrator.DataSeeds;

public class PermissionSeed : ISeed<DContext>
{
	public async Task SeedAsync(DContext context)
	{
		var permissions = GetPermissions();
		var permissionNames = permissions.Select(permission => permission.Name).ToHashSet();
		var existingPermissions = await context.Permissions.ToListAsync();
		var existingNames = existingPermissions.Select(permission => permission.Name).ToHashSet();
		var obsoletePermissions = existingPermissions.Where(permission => !permissionNames.Contains(permission.Name)).ToArray();

		if (obsoletePermissions.Length > 0)
		{
			var obsoleteNames = obsoletePermissions.Select(permission => permission.Name).ToList();

			context.Set<RolePermission>().RemoveRange(await context.Set<RolePermission>()
				.Where(assignment => obsoleteNames.Contains(assignment.PermissionName))
				.ToListAsync());

			context.UserPermissions.RemoveRange(await context.UserPermissions
				.Where(assignment => obsoleteNames.Contains(assignment.Permission))
				.ToListAsync());

			context.Permissions.RemoveRange(obsoletePermissions);
		}

		var newPermissions = permissions.Where(permission => !existingNames.Contains(permission.Name)).ToArray();
		if (newPermissions.Length == 0 && obsoletePermissions.Length == 0)
			return;

		await context.Permissions.AddRangeAsync(newPermissions);
		await context.SaveChangesAsync();
	}

	public int ExecutionOrder => 0;

	private Permission[] GetPermissions() =>
		Enum.GetValues<PermissionCodes>().Select(x => new Permission(x)).ToArray();
}
