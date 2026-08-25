using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Security.Authorization;
using Security.Services;

namespace Infrastructure.Tests.Security;

public class AuthorizationHandlerTests
{
    [Fact]
    public async Task PermissionHandler_WhenAnyPermissionMatches_Succeeds()
    {
        var userContext = CreateUser(permissions: [" purchase_create "]);
        var requirement = new PermissionRequirement(
            ["purchase_create", "purchase_delete"],
            AuthorizationMatch.Any);

        var context = await Authorize(
            new PermissionAuthorizationHandler(userContext),
            requirement);

        Assert.True(context.HasSucceeded);
        Assert.Equal(["PURCHASE.CREATE", "PURCHASE.DELETE"], requirement.Permissions);
    }

    [Fact]
    public async Task PermissionHandler_WhenAllPermissionsMatch_Succeeds()
    {
        var userContext = CreateUser(permissions: ["purchase.create", "purchase_delete"]);
        var requirement = new PermissionRequirement(
            ["PURCHASE_CREATE", "PURCHASE_DELETE"],
            AuthorizationMatch.All);

        var context = await Authorize(
            new PermissionAuthorizationHandler(userContext),
            requirement);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task PermissionHandler_WhenAnAllPermissionIsMissing_DoesNotSucceed()
    {
        var userContext = CreateUser(permissions: ["purchase_create"]);
        var requirement = new PermissionRequirement(
            ["PURCHASE_CREATE", "PURCHASE_DELETE"],
            AuthorizationMatch.All);

        var context = await Authorize(
            new PermissionAuthorizationHandler(userContext),
            requirement);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task PermissionHandler_WhenUserIsAnonymous_DoesNotSucceed()
    {
        var userContext = CreateUser(false, permissions: ["purchase_create"]);
        var requirement = new PermissionRequirement(
            ["PURCHASE_CREATE"],
            AuthorizationMatch.Any);

        var context = await Authorize(
            new PermissionAuthorizationHandler(userContext),
            requirement);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task RoleHandler_NormalizesRolesAndSupportsAllMatch()
    {
        var userContext = CreateUser(roles: [" admin ", "operator"]);
        var requirement = new RoleRequirement(
            ["ADMIN", " Operator "],
            AuthorizationMatch.All);

        var context = await Authorize(
            new RoleAuthorizationHandler(userContext),
            requirement);

        Assert.True(context.HasSucceeded);
        Assert.Equal(["ADMIN", "OPERATOR"], requirement.Roles);
    }

    [Fact]
    public void PermissionRequirement_WhenPermissionsAreEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new PermissionRequirement([], AuthorizationMatch.Any));
    }

    private static async Task<AuthorizationHandlerContext> Authorize(
        IAuthorizationHandler handler,
        IAuthorizationRequirement requirement)
    {
        var context = new AuthorizationHandlerContext(
            [requirement],
            new ClaimsPrincipal(),
            null);

        await handler.HandleAsync(context);
        return context;
    }

    private static UserContext CreateUser(
        bool authenticated = true,
        IEnumerable<string>? permissions = null,
        IEnumerable<string>? roles = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        claims.AddRange((permissions ?? []).Select(x => new Claim("permission", x)));
        claims.AddRange((roles ?? []).Select(x => new Claim(ClaimTypes.Role, x)));

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity(claims, authenticated ? "Test" : null))
        };

        return new UserContext(new HttpContextAccessor { HttpContext = httpContext });
    }
}
