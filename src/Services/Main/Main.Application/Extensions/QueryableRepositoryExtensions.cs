using Main.Entities.Auth.ValueObjects;
using Main.Entities.User;
using Main.Enums;

namespace Main.Application.Extensions;

public static class QueryableRepositoryExtensions
{
    public static IQueryable<User> ExcludeUsersWithRole(this IQueryable<User> query, Role role)
    {
        return query.ExcludeUsersWithRoles([role]);
    }

    public static IQueryable<User> ExcludeUsersWithRole(this IQueryable<User> query, string role)
    {
        return query.ExcludeUsersWithRoles([role]);
    }

    public static IQueryable<User> ExcludeUsersWithRoles(this IQueryable<User> query, IEnumerable<Role> roles)
    {
        return ApplyRoleFilter(query, NormalizeMany(roles), false);
    }

    public static IQueryable<User> ExcludeUsersWithRoles(this IQueryable<User> query, IEnumerable<string> roles)
    {
        return ApplyRoleFilter(query, NormalizeMany(roles), false);
    }

    public static IQueryable<User> IncludeUsersWithRole(this IQueryable<User> query, Role role)
    {
        return query.IncludeUsersWithRoles([role]);
    }

    public static IQueryable<User> IncludeUsersWithRole(this IQueryable<User> query, string role)
    {
        return query.IncludeUsersWithRoles([role]);
    }

    public static IQueryable<User> IncludeUsersWithRoles(this IQueryable<User> query, IEnumerable<Role> roles)
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
            ? query.Where(u => u.Roles.Any(r => normalizedRoles.Contains(r.RoleName.Value)))
            : query.Where(u => !u.Roles.Any(r => normalizedRoles.Contains(r.RoleName.Value)));
    }

    private static string Normalize(string role)
    {
        return RoleName.ToNormalized(role);
    }

    private static string Normalize(Role role)
    {
        return Normalize(role.ToString());
    }

    private static IReadOnlyCollection<string> NormalizeMany(IEnumerable<string> roles)
    {
        return roles.Select(Normalize).Distinct().ToArray();
    }

    private static IReadOnlyCollection<string> NormalizeMany(IEnumerable<Role> roles)
    {
        return roles.Select(Normalize).Distinct().ToArray();
    }
}