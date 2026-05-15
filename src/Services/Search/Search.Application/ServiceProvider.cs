using Application.Common;
using Microsoft.Extensions.DependencyInjection;
using Search.Application.Handler.Articles.AddArticle;

namespace Search.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddApplicationBase(typeof(AddArticleCommand).Assembly);

        return services;
    }
}