using System.Linq.Expressions;
using Application.Common.Interfaces.Repositories;
using Main.Entities.User;
using RoleEnum = Main.Enums.Role;

namespace Main.Application.Extensions;

internal static class UserRoleFilter
{
    public static IQueryable<User> Apply(
        IQueryable<User> query,
        IEnumerable<RoleEnum> roles,
        bool include)
    {
        return Apply(query, NormalizeMany(roles), include);
    }

    public static IQueryable<User> Apply(
        IQueryable<User> query,
        IEnumerable<string> roles,
        bool include)
    {
        return Apply(query, NormalizeMany(roles), include);
    }

    public static CriteriaBuilder<User> Apply(
        CriteriaBuilder<User> criteria,
        IEnumerable<RoleEnum> roles,
        bool include)
    {
        return Apply(criteria, NormalizeMany(roles), include);
    }

    public static CriteriaBuilder<User> Apply(
        CriteriaBuilder<User> criteria,
        IEnumerable<string> roles,
        bool include)
    {
        return Apply(criteria, NormalizeMany(roles), include);
    }

    private static IQueryable<User> Apply(
        IQueryable<User> query,
        IReadOnlyCollection<string> normalizedRoles,
        bool include)
    {
        return normalizedRoles.Count == 0
            ? query
            : query.Where(BuildPredicate(normalizedRoles, include));
    }

    private static CriteriaBuilder<User> Apply(
        CriteriaBuilder<User> criteria,
        IReadOnlyCollection<string> normalizedRoles,
        bool include)
    {
        return normalizedRoles.Count == 0
            ? criteria
            : criteria.Where(BuildPredicate(normalizedRoles, include));
    }

    private static Expression<Func<User, bool>> BuildPredicate(
        IReadOnlyCollection<string> normalizedRoles,
        bool include)
    {
        return include
            ? user => user.Roles.Any(role => normalizedRoles.Contains(role.RoleName))
            : user => !user.Roles.Any(role => normalizedRoles.Contains(role.RoleName));
    }

    private static IReadOnlyCollection<string> NormalizeMany(IEnumerable<string> roles)
    {
        return roles.Select(RoleExtensions.ToNormalizedRole).Distinct().ToArray();
    }

    private static IReadOnlyCollection<string> NormalizeMany(IEnumerable<RoleEnum> roles)
    {
        return roles.Select(RoleExtensions.ToNormalizedRole).Distinct().ToArray();
    }
}
