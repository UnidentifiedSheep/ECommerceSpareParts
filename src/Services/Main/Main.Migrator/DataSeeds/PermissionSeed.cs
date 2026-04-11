using Enums;
using Main.Entities;
using Main.Entities.Auth;
using Main.Persistence.Context;
using Persistence.Interfaces;

namespace Main.Migrator.DataSeeds;

public class PermissionSeed : ISeed<DContext>
{
    public async Task SeedAsync(DContext context)
    {
        if (!context.Permissions.Any())
        {
            var permissions = GetPermissions();
            var existingPermissions = context.Permissions
                .Select(p => p.Name)
                .ToHashSet();
            var newPermissions = permissions
                .Where(p => !existingPermissions.Contains(p.Name))
                .ToList();
            if (newPermissions.Count == 0) return;

            await context.Permissions.AddRangeAsync(newPermissions);
            await context.SaveChangesAsync();
        }
    }

    public int GetPriority()
    {
        return 0;
    }

    private Permission[] GetPermissions()
    {
        return
        [
            new Permission(PermissionCodes.ARTICLES_CREATE, "Права на создание артикула"),
            new Permission(PermissionCodes.ARTICLES_EDIT, "Права на редактирование артикула"),
            new Permission(PermissionCodes.ARTICLES_DELETE, "Права на удаление артикула"),
            new Permission(PermissionCodes.ARTICLES_GET_FULL, "Права на поиск артикулов (полная модель)"),
            new Permission(PermissionCodes.ARTICLES_GET_MAIN, "Права на поиск артикулов (минимальная модель)"),

            new Permission(PermissionCodes.ARTICLE_SIZES_CREATE, "Права на установку размеров артикула"),
            new Permission(PermissionCodes.ARTICLE_SIZES_GET, "Права на получение размеров артикула"),
            new Permission(PermissionCodes.ARTICLE_SIZES_UPDATE, "Права на обновление размеров артикула"),
            new Permission(PermissionCodes.ARTICLE_SIZES_DELETE, "Права на удаление размеров артикула"),

            new Permission(PermissionCodes.ARTICLE_WEIGHT_CREATE, "Права на установку веса артикула"),
            new Permission(PermissionCodes.ARTICLE_WEIGHT_GET, "Права на получение веса артикула"),
            new Permission(PermissionCodes.ARTICLE_WEIGHT_UPDATE, "Права на обновление веса артикула"),
            new Permission(PermissionCodes.ARTICLE_WEIGHT_DELETE, "Права на удаление веса артикула"),

            new Permission(PermissionCodes.PRODUCERS_CREATE, "Права на создание производителей"),
            new Permission(PermissionCodes.PRODUCERS_EDIT, "Права на редактирование производителей"),
            new Permission(PermissionCodes.PRODUCERS_DELETE, "Права на удаление производителей"),

            new Permission(PermissionCodes.ARTICLE_CHARACTERISTICS_CREATE,
                "Права на создание характеристик для артикула"),
            new Permission(PermissionCodes.ARTICLE_CHARACTERISTICS_UPDATE,
                "Права на редактирование характеристик для артикула"),
            new Permission(PermissionCodes.ARTICLE_CHARACTERISTICS_DELETE,
                "Права на удаление характеристик для артикула"),

            new Permission(PermissionCodes.ARTICLE_CROSSES_GET, "Права на получение кросс-артикулов артикула"),
            new Permission(PermissionCodes.ARTICLE_CROSSES_CREATE, "Права на создание кросс связи между артикулами"),
            new Permission(PermissionCodes.ARTICLE_CROSSES_DELETE, "Права на удаление кросс-связи между артикулами"),

            new Permission(PermissionCodes.ARTICLE_CONTENT_CREATE, "Права на создание содержимого артикула"),
            new Permission(PermissionCodes.ARTICLE_CONTENT_EDIT, "Права на редактирование содержимого артикула"),
            new Permission(PermissionCodes.ARTICLE_CONTENT_DELETE, "Права на удаление содержимого артикула"),

            new Permission(PermissionCodes.ARTICLE_PAIR_CREATE, "Права на создание пары артикула"),
            new Permission(PermissionCodes.ARTICLE_PAIR_EDIT, "Права на редактирование пары артикула"),
            new Permission(PermissionCodes.ARTICLE_PAIR_DELETE, "Права на удаление пары артикула"),

            new Permission(PermissionCodes.ARTICLE_IMAGES_CREATE, "Права на добавление изображения артикулу"),
            new Permission(PermissionCodes.ARTICLE_IMAGES_DELETE, "Права на удаление изображения артикула"),

            new Permission(PermissionCodes.ARTICLE_RESERVATIONS_CREATE, "Права на создание резервации артикула"),
            new Permission(PermissionCodes.ARTICLE_RESERVATIONS_EDIT, "Права на редактирование резервации артикула"),
            new Permission(PermissionCodes.ARTICLE_RESERVATIONS_DELETE, "Права на удаление резервации артикула"),
            new Permission(PermissionCodes.ARTICLE_RESERVATIONS_GET_ME, "Права на получение своих резерваций"),
            new Permission(PermissionCodes.ARTICLE_RESERVATIONS_GET_ALL, "Права на получение всех резерваций"),

            new Permission(PermissionCodes.BALANCES_TRANSACTION_CREATE, "Права на создание транзакции"),
            new Permission(PermissionCodes.BALANCES_TRANSACTION_EDIT, "Права на редактирование транзакции"),
            new Permission(PermissionCodes.BALANCES_TRANSACTION_DELETE, "Права на удаление транзакции"),
            new Permission(PermissionCodes.BALANCES_TRANSACTION_GET_ALL, "Права на получение всех транзакций"),
            new Permission(PermissionCodes.BALANCES_TRANSACTION_GET_ME, "Права на получение своих транзакций"),

            new Permission(PermissionCodes.CURRENCIES_CREATE, "Права на создание валюты"),
            new Permission(PermissionCodes.CURRENCIES_GET, "Права на получение валют"),

            new Permission(PermissionCodes.MARKUP_CREATE, "Права на создание правил наценки"),
            new Permission(PermissionCodes.MARKUP_DELETE, "Права на удаление правил наценки"),
            new Permission(PermissionCodes.MARKUP_GET, "Права на получение правил наценки"),
            new Permission(PermissionCodes.MARKUP_SET_DEFAULT, "Права на установку стандартных правил наценки"),

            new Permission(PermissionCodes.PRICES_GET_DETAILED, "Права на получение детальных цен"),
            new Permission(PermissionCodes.PRICES_GET_STANDARD, "Права на получение цен"),

            new Permission(PermissionCodes.PURCHASE_CREATE, "Права на создание покупки"),
            new Permission(PermissionCodes.PURCHASE_EDIT, "Права на редактирование закупки"),
            new Permission(PermissionCodes.PURCHASE_DELETE, "Права на удаление закупки"),
            new Permission(PermissionCodes.PURCHASE_GET, "Права на получение любой закупки"),

            new Permission(PermissionCodes.ROLES_CREATE, "Права на создание роли"),
            new Permission(PermissionCodes.ROLES_GET, "Права на получение роли"),
            new Permission(PermissionCodes.ROLES_PERMISSIONS_CREATE, "Права на добавление прав доступа в роль"),

            new Permission(PermissionCodes.SALES_CREATE, "Права на создание продажи"),
            new Permission(PermissionCodes.SALES_EDIT, "Права на редактирование продажи"),
            new Permission(PermissionCodes.SALES_DELETE, "Права на удаление продажи"),
            new Permission(PermissionCodes.SALES_GET, "Права на получение любой продажи"),

            new Permission(PermissionCodes.STORAGES_CREATE, "Права на создание склада"),
            new Permission(PermissionCodes.STORAGES_EDIT, "Права на редактирование склада"),
            new Permission(PermissionCodes.STORAGES_DELETE, "Права на удаление склада"),
            new Permission(PermissionCodes.STORAGES_GET, "Права на получение всех складов"),

            new Permission(PermissionCodes.STORAGES_CONTENT_CREATE, "Права на добавление содержимого на склад"),
            new Permission(PermissionCodes.STORAGES_CONTENT_EDIT, "Права на редактирование содержимого склада"),
            new Permission(PermissionCodes.STORAGES_CONTENT_DELETE, "Права на удаление содержимого склада"),
            new Permission(PermissionCodes.STORAGES_CONTENT_GET_STANDARD,
                "Права на получение базовых данных содержимого склада"),
            new Permission(PermissionCodes.STORAGES_CONTENT_GET_ALL,
                "Права на получение всех данных содержимого склада"),

            new Permission(PermissionCodes.USERS_CREATE, "Права на создание пользователя"),
            new Permission(PermissionCodes.USERS_GET, "Права на получение всех пользователей"),
            new Permission(PermissionCodes.USERS_INFO_GET, "Права на получение информации о пользователе"),
            new Permission(PermissionCodes.USERS_PERMISSIONS_CREATE, "Права на добавление прав доступа пользователю"),
            new Permission(PermissionCodes.USERS_DISCOUNT_CREATE, "Права на редактирование скидки пользователя"),
            new Permission(PermissionCodes.USERS_DISCOUNT_GET, "Права на получение скидки пользователя"),
            new Permission(PermissionCodes.USERS_MAILS_CREATE,
                "Права на создание корпоративной эл. почты для пользователя"),
            new Permission(PermissionCodes.USERS_VEHICLES_CREATE_ME, "Права на добавление ТС в свой гараж"),
            new Permission(PermissionCodes.USERS_VEHICLES_CREATE_ALL, "Права на добавление ТС в любой гараж"),
            new Permission(PermissionCodes.USERS_STORAGES_ADD, "Права на добавление склада пользователю"),
            new Permission(PermissionCodes.USERS_STORAGES_GET, "Права на получение складов пользователя"),
            new Permission(PermissionCodes.USERS_STORAGES_DELETE, "Права на удаление склада пользователя"),

            new Permission(PermissionCodes.PERMISSIONS_CREATE, "Права на создание прав доступа"),
            new Permission(PermissionCodes.PERMISSIONS_GET, "Права на получение всех прав доступа"),

            new Permission(PermissionCodes.OPTIONS_GET, "Права на получение настроек"),

            new Permission(PermissionCodes.STORAGE_ROUTES_GET, "Права на получение маршрутов"),
            new Permission(PermissionCodes.STORAGE_ROUTES_CREATE, "Права на создание маршрутов"),
            new Permission(PermissionCodes.STORAGE_ROUTES_EDIT, "Права на редактирование маршрутов"),
            new Permission(PermissionCodes.STORAGE_ROUTES_DELETE, "Права на удаление маршрутов"),
            new Permission(PermissionCodes.LOGISTICS_CALCULATE, "Права на получение расчетов логистики.")
        ];
    }
}