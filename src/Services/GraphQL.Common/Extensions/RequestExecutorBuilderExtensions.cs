using GraphQL.Common.Attributes;
using GraphQL.Common.Authorization;
using GraphQL.Common.Enums;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Security.Authorization;

namespace GraphQL.Common.Extensions;

public static class RequestExecutorBuilderExtensions
{
    public static IRequestExecutorBuilder AddCommonAuthorization(
        this IRequestExecutorBuilder builder)
    {
        builder.AddAuthorization(options =>
        {
            options.AddPolicy(
                RequireAnyPermissionAttribute.PolicyName,
                policy => policy.AddRequirements(
                    new GraphQlFieldAuthorizationRequirement(
                        GraphQlAuthorizationTarget.Permission,
                        AuthorizationMatch.Any)));
            options.AddPolicy(
                RequireAllPermissionsAttribute.PolicyName,
                policy => policy.AddRequirements(
                    new GraphQlFieldAuthorizationRequirement(
                        GraphQlAuthorizationTarget.Permission,
                        AuthorizationMatch.All)));
            options.AddPolicy(
                RequireAnyRoleAttribute.PolicyName,
                policy => policy.AddRequirements(
                    new GraphQlFieldAuthorizationRequirement(
                        GraphQlAuthorizationTarget.Role,
                        AuthorizationMatch.Any)));
            options.AddPolicy(
                RequireAllRolesAttribute.PolicyName,
                policy => policy.AddRequirements(
                    new GraphQlFieldAuthorizationRequirement(
                        GraphQlAuthorizationTarget.Role,
                        AuthorizationMatch.All)));
        });
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Scoped<
                IAuthorizationHandler,
                GraphQlFieldAuthorizationHandler>());

        return builder;
    }
}
