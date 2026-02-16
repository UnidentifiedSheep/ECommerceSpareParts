using Application.Common.Aot.Behaviors;
using Application.Common.Aot.Interfaces;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Sannr.AspNetCore;
using Search.Application.Handler.Articles.AddArticle;

namespace Search.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddMediator();
        services.AddSannr();
        

        //Add article command
        services.AddTransient<IPipelineBehavior<AddArticleCommand, Unit>,
            ValidationBehavior<AddArticleCommand, Unit>>();
        services.AddTransient<IValidation<AddArticleCommand>, AddArticleValidation>();
        
        return services;
    }
}