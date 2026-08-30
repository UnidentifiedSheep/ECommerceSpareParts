using Abstractions.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Security.Authorization;

public sealed class RoleAuthorizationHandler(IUserContext userContext) : AuthorizationHandler<RoleRequirement>
{
	protected override Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		RoleRequirement requirement)
	{
		if (!userContext.IsAuthenticated)
			return Task.CompletedTask;

		var allowed = requirement.Match == AuthorizationMatch.Any
			? requirement.Roles.Any(userContext.Roles.Contains)
			: requirement.Roles.All(userContext.Roles.Contains);

		if (allowed)
			context.Succeed(requirement);

		return Task.CompletedTask;
	}
}
