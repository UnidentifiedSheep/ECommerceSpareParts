using Application.Common.Interfaces.Repositories;
using Main.Entities.User;
using RoleEnum = Enums.Role;

namespace Main.Application.Extensions;

public static class UserCriteriaBuilderExtensions
{
    public static CriteriaBuilder<User> WhereHasRole(
        this CriteriaBuilder<User> criteria,
        RoleEnum role)
    {
        return criteria.WhereHasAnyRole([role]);
    }

    public static CriteriaBuilder<User> WhereHasRole(
        this CriteriaBuilder<User> criteria,
        string role)
    {
        return criteria.WhereHasAnyRole([role]);
    }

    public static CriteriaBuilder<User> WhereHasAnyRole(
        this CriteriaBuilder<User> criteria,
        IEnumerable<RoleEnum> roles)
    {
        return UserRoleFilter.Apply(criteria, roles, true);
    }

    public static CriteriaBuilder<User> WhereHasAnyRole(
        this CriteriaBuilder<User> criteria,
        IEnumerable<string> roles)
    {
        return UserRoleFilter.Apply(criteria, roles, true);
    }

    public static CriteriaBuilder<User> WhereDoesNotHaveRole(
        this CriteriaBuilder<User> criteria,
        RoleEnum role)
    {
        return criteria.WhereDoesNotHaveAnyRole([role]);
    }

    public static CriteriaBuilder<User> WhereDoesNotHaveRole(
        this CriteriaBuilder<User> criteria,
        string role)
    {
        return criteria.WhereDoesNotHaveAnyRole([role]);
    }

    public static CriteriaBuilder<User> WhereDoesNotHaveAnyRole(
        this CriteriaBuilder<User> criteria,
        IEnumerable<RoleEnum> roles)
    {
        return UserRoleFilter.Apply(criteria, roles, false);
    }

    public static CriteriaBuilder<User> WhereDoesNotHaveAnyRole(
        this CriteriaBuilder<User> criteria,
        IEnumerable<string> roles)
    {
        return UserRoleFilter.Apply(criteria, roles, false);
    }
}
