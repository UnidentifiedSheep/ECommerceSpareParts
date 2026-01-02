using Core.Extensions;
using Main.Core.Entities;
using Main.Core.Enums;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Persistence.DataSeeds;

public class RolePermissionSeed : ISeed<DContext>
{
    private static IReadOnlyDictionary<string, PermissionCodes[]> BuildRolePermissions() =>
        new Dictionary<string, PermissionCodes[]>
        {
            ["ADMIN"] =
            [
                PermissionCodes.STORAGES_CONTENT_GET_ALL,
                PermissionCodes.STORAGES_GET,
                PermissionCodes.USERS_PERMISSIONS_CREATE,
                PermissionCodes.USERS_VEHICLES_CREATE_ME,
                PermissionCodes.USERS_VEHICLES_CREATE_ALL,
                PermissionCodes.ARTICLES_CREATE,
                PermissionCodes.USERS_MAILS_CREATE,
                PermissionCodes.USERS_CREATE,
                PermissionCodes.USERS_GET,
                PermissionCodes.PERMISSIONS_GET,
                PermissionCodes.PERMISSIONS_CREATE,
                PermissionCodes.USERS_DISCOUNT,
                PermissionCodes.ARTICLES_EDIT,
                PermissionCodes.ARTICLES_DELETE,
                PermissionCodes.ARTICLES_GET_FULL,
                PermissionCodes.ARTICLES_GET_MAIN,
                PermissionCodes.PRODUCERS_CREATE,
                PermissionCodes.PRODUCERS_EDIT,
                PermissionCodes.PRODUCERS_DELETE,
                PermissionCodes.ARTICLE_CHARACTERISTICS_CREATE,
                PermissionCodes.ARTICLE_CHARACTERISTICS_UPDATE,
                PermissionCodes.ARTICLE_CHARACTERISTICS_DELETE,
                PermissionCodes.ARTICLE_CROSSES_GET,
                PermissionCodes.ARTICLE_CROSSES_CREATE,
                PermissionCodes.ARTICLE_CROSSES_DELETE,
                PermissionCodes.ARTICLE_CONTENT_CREATE,
                PermissionCodes.ARTICLE_CONTENT_DELETE,
                PermissionCodes.ARTICLE_CONTENT_EDIT,
                PermissionCodes.ARTICLE_PAIR_EDIT,
                PermissionCodes.ARTICLE_PAIR_CREATE,
                PermissionCodes.ARTICLE_PAIR_DELETE,
                PermissionCodes.ARTICLE_IMAGES_CREATE,
                PermissionCodes.ARTICLE_IMAGES_DELETE,
                PermissionCodes.ARTICLE_RESERVATIONS_CREATE,
                PermissionCodes.ARTICLE_RESERVATIONS_DELETE,
                PermissionCodes.ARTICLE_RESERVATIONS_EDIT,
                PermissionCodes.ARTICLE_RESERVATIONS_GET_ME,
                PermissionCodes.ARTICLE_RESERVATIONS_GET_ALL,
                PermissionCodes.BALANCES_TRANSACTION_CREATE,
                PermissionCodes.BALANCES_TRANSACTION_DELETE,
                PermissionCodes.BALANCES_TRANSACTION_GET_ALL,
                PermissionCodes.BALANCES_TRANSACTION_GET_ME,
                PermissionCodes.BALANCES_TRANSACTION_EDIT,
                PermissionCodes.CURRENCIES_CREATE,
                PermissionCodes.CURRENCIES_GET,
                PermissionCodes.MARKUP_CREATE,
                PermissionCodes.MARKUP_DELETE,
                PermissionCodes.MARKUP_GET,
                PermissionCodes.MARKUP_SET_DEFAULT,
                PermissionCodes.PRICES_GET_DETAILED,
                PermissionCodes.PRICES_GET_STANDART,
                PermissionCodes.PURCHASE_CREATE,
                PermissionCodes.PURCHASE_DELETE,
                PermissionCodes.PURCHASE_EDIT,
                PermissionCodes.PURCHASE_GET,
                PermissionCodes.ROLES_PERMISSIONS_CREATE,
                PermissionCodes.ROLES_CREATE,
                PermissionCodes.ROLES_GET,
                PermissionCodes.SALES_CREATE,
                PermissionCodes.SALES_DELETE,
                PermissionCodes.SALES_EDIT,
                PermissionCodes.SALES_GET,
                PermissionCodes.STORAGES_CONTENT_CREATE,
                PermissionCodes.STORAGES_CREATE,
                PermissionCodes.STORAGES_CONTENT_DELETE,
                PermissionCodes.STORAGES_DELETE,
                PermissionCodes.STORAGES_CONTENT_EDIT,
                PermissionCodes.STORAGES_EDIT,
                PermissionCodes.STORAGES_CONTENT_GET_STANDART
            ],

            ["WORKER"] =
            [
                PermissionCodes.ARTICLE_CHARACTERISTICS_CREATE,
                PermissionCodes.ARTICLE_CHARACTERISTICS_UPDATE,
                PermissionCodes.ARTICLE_CHARACTERISTICS_DELETE,
                PermissionCodes.ARTICLE_CROSSES_GET,
                PermissionCodes.ARTICLE_CROSSES_CREATE,
                PermissionCodes.ARTICLE_CROSSES_DELETE,
                PermissionCodes.ARTICLE_CONTENT_CREATE,
                PermissionCodes.ARTICLE_CONTENT_DELETE,
                PermissionCodes.ARTICLES_CREATE,
                PermissionCodes.ARTICLE_CONTENT_EDIT,
                PermissionCodes.ARTICLE_PAIR_EDIT,
                PermissionCodes.ARTICLE_PAIR_CREATE,
                PermissionCodes.ARTICLE_PAIR_DELETE,
                PermissionCodes.ARTICLE_IMAGES_CREATE,
                PermissionCodes.ARTICLE_IMAGES_DELETE,
                PermissionCodes.ARTICLE_RESERVATIONS_CREATE,
                PermissionCodes.ARTICLE_RESERVATIONS_DELETE,
                PermissionCodes.ARTICLE_RESERVATIONS_EDIT,
                PermissionCodes.ARTICLE_RESERVATIONS_GET_ME,
                PermissionCodes.ARTICLE_RESERVATIONS_GET_ALL,
                PermissionCodes.BALANCES_TRANSACTION_CREATE,
                PermissionCodes.BALANCES_TRANSACTION_DELETE,
                PermissionCodes.BALANCES_TRANSACTION_GET_ALL,
                PermissionCodes.BALANCES_TRANSACTION_GET_ME,
                PermissionCodes.BALANCES_TRANSACTION_EDIT,
                PermissionCodes.CURRENCIES_GET,
                PermissionCodes.PRICES_GET_DETAILED,
                PermissionCodes.PRICES_GET_STANDART,
                PermissionCodes.PURCHASE_CREATE,
                PermissionCodes.PURCHASE_DELETE,
                PermissionCodes.PURCHASE_EDIT,
                PermissionCodes.PURCHASE_GET,
                PermissionCodes.SALES_CREATE,
                PermissionCodes.SALES_DELETE,
                PermissionCodes.SALES_EDIT,
                PermissionCodes.SALES_GET,
                PermissionCodes.STORAGES_CONTENT_CREATE,
                PermissionCodes.STORAGES_CONTENT_EDIT,
                PermissionCodes.STORAGES_EDIT,
                PermissionCodes.STORAGES_CONTENT_GET_STANDART,
                PermissionCodes.STORAGES_CONTENT_GET_ALL,
                PermissionCodes.STORAGES_GET,
                PermissionCodes.USERS_VEHICLES_CREATE_ME,
                PermissionCodes.USERS_VEHICLES_CREATE_ALL,
                PermissionCodes.USERS_GET,
                PermissionCodes.ARTICLES_DELETE,
                PermissionCodes.ARTICLES_EDIT,
                PermissionCodes.ARTICLES_GET_FULL,
                PermissionCodes.ARTICLES_GET_MAIN,
                PermissionCodes.PRODUCERS_CREATE,
                PermissionCodes.PRODUCERS_EDIT,
                PermissionCodes.PRODUCERS_DELETE
            ],

            ["MEMBER"] =
            [
                PermissionCodes.ARTICLE_CROSSES_GET,
                PermissionCodes.ARTICLE_RESERVATIONS_GET_ME,
                PermissionCodes.BALANCES_TRANSACTION_GET_ME,
                PermissionCodes.STORAGES_CONTENT_GET_STANDART,
                PermissionCodes.USERS_VEHICLES_CREATE_ME,
                PermissionCodes.ARTICLES_GET_MAIN
            ]
        };

    public async Task SeedAsync(DContext context)
    {
        var roles = await context.Roles
            .Where(r => r.NormalizedName == "ADMIN" || r.NormalizedName == "WORKER" || r.NormalizedName == "MEMBER")
            .ToListAsync();

        if (roles.Count == 0)
            return;

        var permissions = await context.Permissions
            .ToDictionaryAsync(p => p.Name);

        var rolePermissions = BuildRolePermissions();

        foreach (var role in roles)
        {
            if (!rolePermissions.TryGetValue(role.NormalizedName, out var needed))
                continue;

            role.PermissionNames = ResolvePermissions(needed, permissions);
        }

        await context.SaveChangesAsync();
    }


    private static List<Permission> ResolvePermissions(IEnumerable<PermissionCodes> needed, IReadOnlyDictionary<string, Permission> permissions)
    {
        var result = new List<Permission>();

        foreach (var code in needed)
        {
            var key = code.ToNormalizedPermission();

            if (!permissions.TryGetValue(key, out var permission))
                throw new InvalidOperationException(
                    $"Permission '{key}' not found in database");

            result.Add(permission);
        }

        return result;
    }

    public int GetPriority() => 1;
}
