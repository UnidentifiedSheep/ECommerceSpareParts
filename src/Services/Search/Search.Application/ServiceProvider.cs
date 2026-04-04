using Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Search.Application.Handler.Articles.SearchArticles;

namespace Search.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(SearchArticlesHandler).Assembly);
        
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(SearchArticlesHandler).Assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(RequestsDataLoggingBehavior<,>));
            config.AddOpenBehavior(typeof(CacheBehavior<,>));
            config.AddOpenBehavior(typeof(TransactionBehavior<,>), ServiceLifetime.Scoped);
            config.AddOpenBehavior(typeof(DbValidationBehavior<,>), ServiceLifetime.Scoped);
        });

        return services;
    }
}