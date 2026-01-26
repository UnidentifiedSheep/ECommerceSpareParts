using Main.Entities;
using Main.Enums;
using Main.Persistence.Context;
using Persistence.Interfaces;

namespace Main.Persistence.DataSeeds;

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

    public int GetPriority() => 0;

    private Permission[] GetPermissions()
{
    return
    [
        new(PermissionCodes.ARTICLES_CREATE, "Права на создание артикула"),
        new(PermissionCodes.ARTICLES_EDIT, "Права на редактирование артикула"),
        new(PermissionCodes.ARTICLES_DELETE, "Права на удаление артикула"),
        new(PermissionCodes.ARTICLES_GET_FULL, "Права на поиск артикулов (полная модель)"),
        new(PermissionCodes.ARTICLES_GET_MAIN, "Права на поиск артикулов (минимальная модель)"),

        new(PermissionCodes.PRODUCERS_CREATE, "Права на создание производителей"),
        new(PermissionCodes.PRODUCERS_EDIT, "Права на редактирование производителей"),
        new(PermissionCodes.PRODUCERS_DELETE, "Права на удаление производителей"),

        new(PermissionCodes.ARTICLE_CHARACTERISTICS_CREATE, "Права на создание характеристик для артикула"),
        new(PermissionCodes.ARTICLE_CHARACTERISTICS_UPDATE, "Права на редактирование характеристик для артикула"),
        new(PermissionCodes.ARTICLE_CHARACTERISTICS_DELETE, "Права на удаление характеристик для артикула"),

        new(PermissionCodes.ARTICLE_CROSSES_GET, "Права на получение кросс-артикулов артикула"),
        new(PermissionCodes.ARTICLE_CROSSES_CREATE, "Права на создание кросс связи между артикулами"),
        new(PermissionCodes.ARTICLE_CROSSES_DELETE, "Права на удаление кросс-связи между артикулами"),

        new(PermissionCodes.ARTICLE_CONTENT_CREATE, "Права на создание содержимого артикула"),
        new(PermissionCodes.ARTICLE_CONTENT_EDIT, "Права на редактирование содержимого артикула"),
        new(PermissionCodes.ARTICLE_CONTENT_DELETE, "Права на удаление содержимого артикула"),

        new(PermissionCodes.ARTICLE_PAIR_CREATE, "Права на создание пары артикула"),
        new(PermissionCodes.ARTICLE_PAIR_EDIT, "Права на редактирование пары артикула"),
        new(PermissionCodes.ARTICLE_PAIR_DELETE, "Права на удаление пары артикула"),

        new(PermissionCodes.ARTICLE_IMAGES_CREATE, "Права на добавление изображения артикулу"),
        new(PermissionCodes.ARTICLE_IMAGES_DELETE, "Права на удаление изображения артикула"),

        new(PermissionCodes.ARTICLE_RESERVATIONS_CREATE, "Права на создание резервации артикула"),
        new(PermissionCodes.ARTICLE_RESERVATIONS_EDIT, "Права на редактирование резервации артикула"),
        new(PermissionCodes.ARTICLE_RESERVATIONS_DELETE, "Права на удаление резервации артикула"),
        new(PermissionCodes.ARTICLE_RESERVATIONS_GET_ME, "Права на получение своих резерваций"),
        new(PermissionCodes.ARTICLE_RESERVATIONS_GET_ALL, "Права на получение всех резерваций"),

        new(PermissionCodes.BALANCES_TRANSACTION_CREATE, "Права на создание транзакции"),
        new(PermissionCodes.BALANCES_TRANSACTION_EDIT, "Права на редактирование транзакции"),
        new(PermissionCodes.BALANCES_TRANSACTION_DELETE, "Права на удаление транзакции"),
        new(PermissionCodes.BALANCES_TRANSACTION_GET_ALL, "Права на получение всех транзакций"),
        new(PermissionCodes.BALANCES_TRANSACTION_GET_ME, "Права на получение своих транзакций"),

        new(PermissionCodes.CURRENCIES_CREATE, "Права на создание валюты"),
        new(PermissionCodes.CURRENCIES_GET, "Права на получение валют"),

        new(PermissionCodes.MARKUP_CREATE, "Права на создание правил наценки"),
        new(PermissionCodes.MARKUP_DELETE, "Права на удаление правил наценки"),
        new(PermissionCodes.MARKUP_GET, "Права на получение правил наценки"),
        new(PermissionCodes.MARKUP_SET_DEFAULT, "Права на установку стандартных правил наценки"),

        new(PermissionCodes.PRICES_GET_DETAILED, "Права на получение детальных цен"),
        new(PermissionCodes.PRICES_GET_STANDARD, "Права на получение цен"),

        new(PermissionCodes.PURCHASE_CREATE, "Права на создание покупки"),
        new(PermissionCodes.PURCHASE_EDIT, "Права на редактирование закупки"),
        new(PermissionCodes.PURCHASE_DELETE, "Права на удаление закупки"),
        new(PermissionCodes.PURCHASE_GET, "Права на получение любой закупки"),

        new(PermissionCodes.ROLES_CREATE, "Права на создание роли"),
        new(PermissionCodes.ROLES_GET, "Права на получение роли"),
        new(PermissionCodes.ROLES_PERMISSIONS_CREATE, "Права на добавление прав доступа в роль"),

        new(PermissionCodes.SALES_CREATE, "Права на создание продажи"),
        new(PermissionCodes.SALES_EDIT, "Права на редактирование продажи"),
        new(PermissionCodes.SALES_DELETE, "Права на удаление продажи"),
        new(PermissionCodes.SALES_GET, "Права на получение любой продажи"),

        new(PermissionCodes.STORAGES_CREATE, "Права на создание склада"),
        new(PermissionCodes.STORAGES_EDIT, "Права на редактирование склада"),
        new(PermissionCodes.STORAGES_DELETE, "Права на удаление склада"),
        new(PermissionCodes.STORAGES_GET, "Права на получение всех складов"),

        new(PermissionCodes.STORAGES_CONTENT_CREATE, "Права на добавление содержимого на склад"),
        new(PermissionCodes.STORAGES_CONTENT_EDIT, "Права на редактирование содержимого склада"),
        new(PermissionCodes.STORAGES_CONTENT_DELETE, "Права на удаление содержимого склада"),
        new(PermissionCodes.STORAGES_CONTENT_GET_STANDARD, "Права на получение базовых данных содержимого склада"),
        new(PermissionCodes.STORAGES_CONTENT_GET_ALL, "Права на получение всех данных содержимого склада"),

        new(PermissionCodes.USERS_CREATE, "Права на создание пользователя"),
        new(PermissionCodes.USERS_GET, "Права на получение всех пользователей"),
        new(PermissionCodes.USERS_INFO_GET, "Права на получение информации о пользователе"),
        new(PermissionCodes.USERS_PERMISSIONS_CREATE, "Права на добавление прав доступа пользователю"),
        new(PermissionCodes.USERS_DISCOUNT_CREATE, "Права на редактирование скидки пользователя"),
        new(PermissionCodes.USERS_DISCOUNT_GET, "Права на получение скидки пользователя"),
        new(PermissionCodes.USERS_MAILS_CREATE, "Права на создание корпоративной эл. почты для пользователя"),
        new(PermissionCodes.USERS_VEHICLES_CREATE_ME, "Права на добавление ТС в свой гараж"),
        new(PermissionCodes.USERS_VEHICLES_CREATE_ALL, "Права на добавление ТС в любой гараж"),
        new (PermissionCodes.USERS_STORAGES_ADD, "Права на добавление склада пользователю"),
        new (PermissionCodes.USERS_STORAGES_GET, "Права на получение складов пользователя"),

        new(PermissionCodes.PERMISSIONS_CREATE, "Права на создание прав доступа"),
        new(PermissionCodes.PERMISSIONS_GET, "Права на получение всех прав доступа"),

        new(PermissionCodes.OPTIONS_GET, "Права на получение настроек"),
        
        new Permission(PermissionCodes.STORAGE_ROUTES_GET, "Права на получение маршрутов"),
        new Permission(PermissionCodes.STORAGE_ROUTES_CREATE, "Права на создание маршрутов"),
        new Permission(PermissionCodes.STORAGE_ROUTES_EDIT, "Права на редактирование маршрутов"),
        new Permission(PermissionCodes.STORAGE_ROUTES_DELETE, "Права на удаление маршрутов")
    ];
}

}