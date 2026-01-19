using Main.Entities;
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
            new Permission { Name = "ARTICLES.CREATE", Description = "Права на создание артикула" },
            new Permission { Name = "ARTICLES.EDIT", Description = "Права на редактирование артикула" },
            new Permission { Name = "ARTICLES.DELETE", Description = "Права на удаление артикула" },
            new Permission { Name = "ARTICLES.GET.FULL", Description = "Права на поиск артикулов (полная модель)" },
            new Permission { Name = "ARTICLES.GET.MAIN", Description = "Права на поиск артикулов (минимальная модель)" },

            new Permission { Name = "PRODUCERS.CREATE", Description = "Права на создание производителей" },
            new Permission { Name = "PRODUCERS.EDIT", Description = "Права на редактирование производителей" },
            new Permission { Name = "PRODUCERS.DELETE", Description = "Права на удаление производителей" },

            new Permission { Name = "ARTICLE.CHARACTERISTICS.CREATE", Description = "Права на создание характеристик для артикула" },
            new Permission { Name = "ARTICLE.CHARACTERISTICS.UPDATE", Description = "Права на редактирование характеристик для артикула" },
            new Permission { Name = "ARTICLE.CHARACTERISTICS.DELETE", Description = "Права на удаление характеристик для артикула" },

            new Permission { Name = "ARTICLE.CROSSES.GET", Description = "Права на получение кросс-артикулов артикула" },
            new Permission { Name = "ARTICLE.CROSSES.CREATE", Description = "Права на создание кросс связи между артикулами" },
            new Permission { Name = "ARTICLE.CROSSES.DELETE", Description = "Права на удаление кросссвязи между артикулами" },

            new Permission { Name = "ARTICLE.CONTENT.CREATE", Description = "Права на создание содержимого артикула" },
            new Permission { Name = "ARTICLE.CONTENT.EDIT", Description = "Права на редактирование содержимого артикула" },
            new Permission { Name = "ARTICLE.CONTENT.DELETE", Description = "Права на удаление содержимого артикула" },

            new Permission { Name = "ARTICLE.PAIR.CREATE", Description = "Права на создание пары артикула" },
            new Permission { Name = "ARTICLE.PAIR.EDIT", Description = "Права на редактирование пары артикула" },
            new Permission { Name = "ARTICLE.PAIR.DELETE", Description = "Права на удаление пары артикула" },

            new Permission { Name = "ARTICLE.IMAGES.CREATE", Description = "Права на добавление изображения артикулу" },
            new Permission { Name = "ARTICLE.IMAGES.DELETE", Description = "Права на удаление изображения артикула" },

            new Permission { Name = "ARTICLE.RESERVATIONS.CREATE", Description = "Права на создание резервации артикула" },
            new Permission { Name = "ARTICLE.RESERVATIONS.EDIT", Description = "Права на редактирование резервации артикула" },
            new Permission { Name = "ARTICLE.RESERVATIONS.DELETE", Description = "Права на удаление резервации артикула" },
            new Permission { Name = "ARTICLE.RESERVATIONS.GET.ME", Description = "Права на получение своих резерваций" },
            new Permission { Name = "ARTICLE.RESERVATIONS.GET.ALL", Description = "Права на получение всех резерваций" },

            new Permission { Name = "BALANCES.TRANSACTION.CREATE", Description = "Права на создание транзакции" },
            new Permission { Name = "BALANCES.TRANSACTION.EDIT", Description = "Права на редактирование транзакции" },
            new Permission { Name = "BALANCES.TRANSACTION.DELETE", Description = "Права на удаление транзакции" },
            new Permission { Name = "BALANCES.TRANSACTION.GET.ALL", Description = "Права на получение всех транзакции" },
            new Permission { Name = "BALANCES.TRANSACTION.GET.ME", Description = "Права на получение своих транзакций" },

            new Permission { Name = "CURRENCIES.CREATE", Description = "Права на создание валюты" },
            new Permission { Name = "CURRENCIES.GET", Description = "Права на получение валют/ы" },

            new Permission { Name = "MARKUP.CREATE", Description = "Права на создание правил наценки" },
            new Permission { Name = "MARKUP.DELETE", Description = "Права на удаление правил наценки" },
            new Permission { Name = "MARKUP.GET", Description = "Права на получения правил наценки" },
            new Permission { Name = "MARKUP.SET.DEFAULT", Description = "Права на установку стандартных правил наценки" },

            new Permission { Name = "PRICES.GET.DETAILED", Description = "Права на получение детальных цен" },
            new Permission { Name = "PRICES.GET.STANDART", Description = "Права на получение цен" },

            new Permission { Name = "PURCHASE.CREATE", Description = "Права на создание покупки" },
            new Permission { Name = "PURCHASE.EDIT", Description = "Права на редактирование закупки" },
            new Permission { Name = "PURCHASE.DELETE", Description = "Права на удаление закупки" },
            new Permission { Name = "PURCHASE.GET", Description = "Права на получение любой закупки" },

            new Permission { Name = "ROLES.CREATE", Description = "Права на создание роли" },
            new Permission { Name = "ROLES.GET", Description = "Права на получение роли" },
            new Permission { Name = "ROLES.PERMISSIONS.CREATE", Description = "Права на добавление прав доступа в роль" },

            new Permission { Name = "SALES.CREATE", Description = "Права на создание продажи" },
            new Permission { Name = "SALES.EDIT", Description = "Права на редактирование продажи" },
            new Permission { Name = "SALES.DELETE", Description = "Права на удаление продажи" },
            new Permission { Name = "SALES.GET", Description = "Права на получение любой продажи" },

            new Permission { Name = "STORAGES.CREATE", Description = "Права на создание склада" },
            new Permission { Name = "STORAGES.EDIT", Description = "Права на редактирование склада" },
            new Permission { Name = "STORAGES.DELETE", Description = "Права на удаление склада" },
            new Permission { Name = "STORAGES.GET", Description = "Права на получение всех складов" },

            new Permission { Name = "STORAGES.CONTENT.CREATE", Description = "Права на добавление содержимого на склад" },
            new Permission { Name = "STORAGES.CONTENT.EDIT", Description = "Права на редактирование содержимого склада" },
            new Permission { Name = "STORAGES.CONTENT.DELETE", Description = "Права на удаление сождержимого склада" },
            new Permission { Name = "STORAGES.CONTENT.GET.STANDART", Description = "Права на получение базовых данных содержимого склада" },
            new Permission { Name = "STORAGES.CONTENT.GET.ALL", Description = "Права на получение всех данных содержимого склада" },

            new Permission { Name = "USERS.CREATE", Description = "Права на создание пользователя" },
            new Permission { Name = "USERS.GET", Description = "Права на получение всех пользователей" },
            new Permission { Name = "USERS.PERMISSIONS.CREATE", Description = "Права на добавление прав доступа пользователю" },
            new Permission { Name = "USERS.DISCOUNT", Description = "Права на редактирование скидки пользователя" },
            new Permission { Name = "USERS.MAILS.CREATE", Description = "Права на создание корпоративной эл. почты для пользователя" },
            new Permission { Name = "USERS.VEHICLES.CREATE.ME", Description = "Права на добавление ТС в свой гараж" },
            new Permission { Name = "USERS.VEHICLES.CREATE.ALL", Description = "Права на добавление ТС в любой гараж" },

            new Permission { Name = "PERMISSIONS.CREATE", Description = "Права на создание прав доступа" },
            new Permission { Name = "PERMISSIONS.GET", Description = "Права на получение всех прав доступа" }
        ];
    }
}