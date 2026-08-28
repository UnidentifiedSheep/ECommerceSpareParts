using Microsoft.AspNetCore.Authorization;

namespace GraphQL.Common.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
public abstract class RequireAuthorizationAttribute
    : HotChocolate.Authorization.AuthorizeAttribute
{
    protected RequireAuthorizationAttribute(
        string policy,
        IAuthorizationRequirement requirement)
        : base(policy)
    {
        Requirement = requirement;
    }

    public IAuthorizationRequirement Requirement { get; }
}
