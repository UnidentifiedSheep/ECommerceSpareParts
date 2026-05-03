using Main.Entities.Auth.ValueObjects;
using Main.Entities.User;
using Main.Enums;

namespace Main.Application.Extensions;

public static class QueryableRepositoryExtensions
{
    public static IQueryable<User> ExcludeUsersWithRole(this IQueryable<User> query, Role role)
        => ExcludeUsersWithRoles(query, [role]);

    public static IQueryable<User> ExcludeUsersWithRole(this IQueryable<User> query, string role)
        => ExcludeUsersWithRoles(query, [role]);

    public static IQueryable<User> ExcludeUsersWithRoles(this IQueryable<User> query, IEnumerable<Role> roles)
        => ApplyRoleFilter(query, NormalizeMany(roles), include: false);

    public static IQueryable<User> ExcludeUsersWithRoles(this IQueryable<User> query, IEnumerable<string> roles)
        => ApplyRoleFilter(query, NormalizeMany(roles), include: false);

    public static IQueryable<User> IncludeUsersWithRole(this IQueryable<User> query, Role role)
        => IncludeUsersWithRoles(query, [role]);

    public static IQueryable<User> IncludeUsersWithRole(this IQueryable<User> query, string role)
        => IncludeUsersWithRoles(query, [role]);

    public static IQueryable<User> IncludeUsersWithRoles(this IQueryable<User> query, IEnumerable<Role> roles)
        => ApplyRoleFilter(query, NormalizeMany(roles), include: true);

    public static IQueryable<User> IncludeUsersWithRoles(this IQueryable<User> query, IEnumerable<string> roles)
        => ApplyRoleFilter(query, NormalizeMany(roles), include: true);

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
        => RoleName.ToNormalized(role);

    private static string Normalize(Role role)
        => Normalize(role.ToString());

    private static IReadOnlyCollection<string> NormalizeMany(IEnumerable<string> roles)
        => roles.Select(Normalize).Distinct().ToArray();

    private static IReadOnlyCollection<string> NormalizeMany(IEnumerable<Role> roles)
        => roles.Select(Normalize).Distinct().ToArray();
}