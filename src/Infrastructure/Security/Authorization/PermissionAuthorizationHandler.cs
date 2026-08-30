using Abstractions.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Security.Authorization;

public sealed class PermissionAuthorizationHandler(IUserContext userContext)
	: AuthorizationHandler<PermissionRequirement>
{
	protected override Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		PermissionRequirement requirement)
	{
		if (!userContext.IsAuthenticated)
			return Task.CompletedTask;

		var allowed = requirement.Match == AuthorizationMatch.Any
			? requirement.Permissions.Any(userContext.Permissions.Contains)
			: requirement.Permissions.All(userContext.Permissions.Contains);

		if (allowed)
			context.Succeed(requirement);

		return Task.CompletedTask;
	}
}
