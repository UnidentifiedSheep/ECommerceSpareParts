using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Persistence.Repositories;
using Persistence.Services;

namespace Persistence;

public static class ServiceProvider
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection collection, string connectionString)
    {
        collection.AddDbContext<DContext>(options => options.UseNpgsql(connectionString));
        collection.AddDbContext<IdentityContext>(options => options.UseNpgsql(connectionString));
        
        collection.AddScoped<IUserVehicleRepository, UserVehicleRepository>();
        collection.AddScoped<IUsersRepository, UsersRepository>();
        collection.AddScoped<IUserEmailRepository, UserEmailRepository>();
        collection.AddScoped<IStoragesRepository, StoragesRepository>();
        collection.AddScoped<IStorageContentRepository, StorageContentRepository>();
        collection.AddScoped<ISaleRepository, SaleRepository>();
        collection.AddScoped<IPurchaseRepository, PurchaseRepository>();
        collection.AddScoped<IProducerRepository, ProducerRepository>();
        collection.AddScoped<IMarkupRepository, MarkupRepository>();
        collection.AddScoped<IDefaultSettingsRepository, DefaultSettingsRepository>();
        collection.AddScoped<ICurrencyRepository, CurrencyRepository>();
        collection.AddScoped<IBuySellPriceRepository, BuySellPriceRepository>();
        collection.AddScoped<IBalanceRepository, BalanceRepository>();
        collection.AddScoped<IArticleImageRepository, ArticleImageRepository>();
        collection.AddScoped<IArticlesRepository, ArticlesRepository>();
        collection.AddScoped<IArticleReservationRepository, ArticleReservationRepository>();
        collection.AddScoped<IArticlePairsRepository, ArticlePairsRepository>();
        collection.AddScoped<IArticleContentRepository, ArticleContentRepository>();
        collection.AddScoped<IArticleCharacteristicsRepository, ArticleCharacteristicsRepository>();

        collection.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return collection;
    }
}