using System.Reflection;
using Abstractions.Interfaces;
using GraphQL.Common.Attributes;
using GraphQL.Common.Enums;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;
using Security.Authorization;

namespace GraphQL.Common.Authorization;

internal sealed class GraphQlFieldAuthorizationHandler(IUserContext userContext)
	: AuthorizationHandler<GraphQlFieldAuthorizationRequirement, IResolverContext>
{
	private readonly PermissionAuthorizationHandler _permissionHandler = new(userContext);

	private readonly RoleAuthorizationHandler _roleHandler = new(userContext);

	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		GraphQlFieldAuthorizationRequirement requirement,
		IResolverContext resolverContext)
	{
		var member = resolverContext.Selection.Field.ResolverMember ?? resolverContext.Selection.Field.Member;
		if (member is null)
			return;

		var requirements = member
			.GetCustomAttributes<RequireAuthorizationAttribute>(true)
			.Select(x => x.Requirement)
			.Where(x => Matches(x, requirement))
			.ToArray();
		if (requirements.Length == 0)
			return;

		foreach (var fieldRequirement in requirements)
		{
			var nestedContext = new AuthorizationHandlerContext(
				[fieldRequirement],
				context.User,
				resolverContext);

			if (fieldRequirement is PermissionRequirement)
				await _permissionHandler.HandleAsync(nestedContext);
			else
				await _roleHandler.HandleAsync(nestedContext);

			if (!nestedContext.HasSucceeded)
				return;
		}

		context.Succeed(requirement);
	}

	private static bool Matches(
		IAuthorizationRequirement fieldRequirement,
		GraphQlFieldAuthorizationRequirement policyRequirement)
	{
		return (fieldRequirement, policyRequirement.Target) switch
		{
			(PermissionRequirement permission, GraphQlAuthorizationTarget.Permission) => permission.Match ==
				policyRequirement.Match,
			(RoleRequirement role, GraphQlAuthorizationTarget.Role) => role.Match == policyRequirement.Match,
			_ => false
		};
	}
}
