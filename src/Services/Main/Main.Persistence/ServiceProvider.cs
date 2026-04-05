using Abstractions.Interfaces;
using BulkValidation.Pgsql.Extensions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Persistence.Context;
using Main.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.DbValidator;
using Persistence.Extensions;

namespace Main.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection, string connectionString)
    {
        collection.AddDbContext<DContext>(options => options.UseNpgsql(connectionString));

        collection.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
        collection.AddScoped<IPermissionRepository, PermissionsRepository>();
        collection.AddScoped<IUserVehicleRepository, UserVehicleRepository>();
        collection.AddScoped<IUserRepository, UserRepository>();
        collection.AddScoped<IRoleRepository, RoleRepository>();
        collection.AddScoped<IUserEmailRepository, UserEmailRepository>();
        collection.AddScoped<IUserTokenRepository, UserTokenRepository>();
        collection.AddScoped<IUserRoleRepository, UserRoleRepository>();
        collection.AddScoped<IRoleRepository, RoleRepository>();
        collection.AddScoped<IUserEmailRepository, UserEmailRepository>();
        collection.AddScoped<IStoragesRepository, StoragesRepository>();
        collection.AddScoped<IStorageContentRepository, StorageContentRepository>();
        collection.AddScoped<ISaleRepository, SaleRepository>();
        collection.AddScoped<IPurchaseRepository, PurchaseRepository>();
        collection.AddScoped<IProducerRepository, ProducerRepository>();
        collection.AddScoped<ICurrencyRepository, CurrencyRepository>();
        collection.AddScoped<IBalanceRepository, BalanceRepository>();
        collection.AddScoped<IArticleImageRepository, ArticleImageRepository>();
        collection.AddScoped<IArticlesRepository, ArticlesRepository>();
        collection.AddScoped<IArticleReservationRepository, ArticleReservationRepository>();
        collection.AddScoped<IArticlePairsRepository, ArticlePairsRepository>();
        collection.AddScoped<IArticleContentRepository, ArticleContentRepository>();
        collection.AddScoped<IArticleCharacteristicsRepository, ArticleCharacteristicsRepository>();
        collection.AddScoped<ICartRepository, CartRepository>();
        collection.AddScoped<IStorageRoutesRepository, StorageRoutesRepository>();
        collection.AddScoped<IStorageOwnersRepository, StorageOwnersRepository>();
        collection.AddScoped<IArticleWeightRepository, ArticleWeightRepository>();
        collection.AddScoped<IArticleSizesRepository, ArticleSizesRepository>();
        collection.AddScoped<IPurchaseLogisticsRepository, PurchaseLogisticsRepository>();
        collection.AddScoped<IPurchaseContentLogisticsRepository, PurchaseContentLogisticsRepository>();
        collection.AddScoped<IArticleCoefficients, ArticleCoefficients>();
        collection.AddScoped<ISettingsRepository, SettingsRepository>();

        collection.AddUnitOfWork<DContext>();

        collection.AddScoped<IDbValidator, PgsqlDbValidator<DContext>>();
        collection.AddPgsqlDbValidators<DContext>();


        return collection;
    }
}