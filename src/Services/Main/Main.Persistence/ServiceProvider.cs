using Core.Interfaces;
using Core.Interfaces.Services;
using Main.Core.Interfaces;
using Main.Core.Interfaces.DbRepositories;
using Main.Persistence.Context;
using Main.Persistence.DataSeeds;
using Main.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence.Interfaces;

namespace Main.Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection, string connectionString)
    {
        collection.AddDbContext<DContext>(options => options.UseNpgsql(connectionString));
        collection.AddScoped<ICombinedDataLoader, CombinedDataLoader>();

        collection.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
        collection.AddScoped<IPermissionRepository, PermissionsRepository>();
        collection.AddScoped<IUserVehicleRepository, UserVehicleRepository>();
        collection.AddScoped<IUserRepository, UserRepository>();
        collection.AddScoped<IRoleRepository, RoleRepository>();
        collection.AddScoped<IUserPhoneRepository, UserPhoneRepository>();
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
        collection.AddScoped<IMarkupRepository, MarkupRepository>();
        collection.AddScoped<IDefaultSettingsRepository, DefaultSettingsRepository>();
        collection.AddScoped<ICurrencyRepository, CurrencyRepository>();
        collection.AddScoped<IBalanceRepository, BalanceRepository>();
        collection.AddScoped<IArticleImageRepository, ArticleImageRepository>();
        collection.AddScoped<IArticlesRepository, ArticlesRepository>();
        collection.AddScoped<IArticleReservationRepository, ArticleReservationRepository>();
        collection.AddScoped<IArticlePairsRepository, ArticlePairsRepository>();
        collection.AddScoped<IArticleContentRepository, ArticleContentRepository>();
        collection.AddScoped<IArticleCharacteristicsRepository, ArticleCharacteristicsRepository>();

        collection.AddScoped<IUnitOfWork, UnitOfWork>();
        
        //Seeds
        collection.AddScoped<ISeed<DContext>, PermissionSeed>();
        collection.AddScoped<ISeed<DContext>, RoleSeed>();
        collection.AddScoped<ISeed<DContext>, RolePermissionSeed>();
        collection.AddScoped<ISeed<DContext>, UserSeed>();
        collection.AddScoped<ISeed<DContext>, CurrencySeed>();
        
        return collection;
    }
}