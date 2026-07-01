using Main.Entities.User;
using RoleEnum = Enums.Role;

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
        return UserRoleFilter.Apply(query, roles, false);
    }

    public static IQueryable<User> ExcludeUsersWithRoles(this IQueryable<User> query, IEnumerable<string> roles)
    {
        return UserRoleFilter.Apply(query, roles, false);
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
        return UserRoleFilter.Apply(query, roles, true);
    }

    public static IQueryable<User> IncludeUsersWithRoles(this IQueryable<User> query, IEnumerable<string> roles)
    {
        return UserRoleFilter.Apply(query, roles, true);
    }
}
