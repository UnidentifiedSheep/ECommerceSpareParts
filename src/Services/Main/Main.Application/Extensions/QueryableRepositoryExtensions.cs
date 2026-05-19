using Main.Entities.Auth;
using Main.Entities.User;
using RoleEnum = Main.Enums.Role;

namespace Main.Application.Extensions;

public static class QueryableRepositoryExtensions
{
    public static IQueryable<User> ExcludeUsersWithRole(this IQueryable<User> query, RoleEnum role)
    {
        return query.ExcludeUsersWithRoles([role]);
    }

    public static IQueryable<User> ExcludeUsersWithRole(this IQueryable<User> query, string role)
    {
        return query.ExcludeUsersWithRoles([role]);
    }

    public static IQueryable<User> ExcludeUsersWithRoles(this IQueryable<User> query, IEnumerable<RoleEnum> roles)
    {
        return ApplyRoleFilter(query, NormalizeMany(roles), false);
    }

    public static IQueryable<User> ExcludeUsersWithRoles(this IQueryable<User> query, IEnumerable<string> roles)
    {
        return ApplyRoleFilter(query, NormalizeMany(roles), false);
    }

    public static IQueryable<User> IncludeUsersWithRole(this IQueryable<User> query, RoleEnum role)
    {
        return query.IncludeUsersWithRoles([role]);
    }

    public static IQueryable<User> IncludeUsersWithRole(this IQueryable<User> query, string role)
    {
        return query.IncludeUsersWithRoles([role]);
    }

    public static IQueryable<User> IncludeUsersWithRoles(this IQueryable<User> query, IEnumerable<RoleEnum> roles)
    {
        return ApplyRoleFilter(query, NormalizeMany(roles), true);
    }

    public static IQueryable<User> IncludeUsersWithRoles(this IQueryable<User> query, IEnumerable<string> roles)
    {
        return ApplyRoleFilter(query, NormalizeMany(roles), true);
    }

    private static IQueryable<User> ApplyRoleFilter(
        IQueryable<User> query,
        IReadOnlyCollection<string> normalizedRoles,
        bool include)
    {
        if (normalizedRoles.Count == 0) return query;

        return include
            ? query.Where(u => u.Roles.Any(r => normalizedRoles.Contains(r.RoleName)))
            : query.Where(u => !u.Roles.Any(r => normalizedRoles.Contains(r.RoleName)));
    }

    private static string Normalize(string role)
    {
        return RoleNames.Normalize(role);
    }

    private static string Normalize(RoleEnum role)
    {
        return Normalize(role.ToString());
    }

    private static IReadOnlyCollection<string> NormalizeMany(IEnumerable<string> roles)
    {
        return roles.Select(Normalize).Distinct().ToArray();
    }

    private static IReadOnlyCollection<string> NormalizeMany(IEnumerable<RoleEnum> roles)
    {
        return roles.Select(Normalize).Distinct().ToArray();
    }
}
