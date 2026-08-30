using Microsoft.AspNetCore.Authorization;
using AuthorizeAttribute = HotChocolate.Authorization.AuthorizeAttribute;

namespace GraphQL.Common.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
public abstract class RequireAuthorizationAttribute : AuthorizeAttribute
{
	protected RequireAuthorizationAttribute(string policy, IAuthorizationRequirement requirement) : base(
		policy)
	{
		Requirement = requirement;
	}

	public IAuthorizationRequirement Requirement { get; }
}
