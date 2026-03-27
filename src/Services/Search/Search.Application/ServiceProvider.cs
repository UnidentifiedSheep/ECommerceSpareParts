using Application.Common.Aot.Behaviors;
using Application.Common.Aot.Interfaces;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Sannr.AspNetCore;
using Search.Application.Handler.Articles.AddArticle;
using Search.Application.Handler.Articles.GetSuggestions;
using Search.Application.Handler.Articles.SearchArticles;

namespace Search.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddMediator();
        services.AddSannr();

        //Add article command
        services
            .AddTransient<IPipelineBehavior<AddArticleCommand, Unit>, ValidationBehavior<AddArticleCommand, Unit>>();
        services.AddTransient<IValidation<AddArticleCommand>, AddArticleValidation>();

        //Get suggestions query
        services.AddTransient<IPipelineBehavior<GetSuggestionsQuery, GetSuggestionsResult>,
            ValidationBehavior<GetSuggestionsQuery, GetSuggestionsResult>>();
        services.AddTransient<IValidation<GetSuggestionsQuery>, GetSuggestionsValidation>();

        //Search Articles query
        services.AddTransient<IPipelineBehavior<SearchArticlesQuery, SearchArticlesResult>,
            ValidationBehavior<SearchArticlesQuery, SearchArticlesResult>>();
        services.AddTransient<IValidation<SearchArticlesQuery>, SearchArticlesValidation>();

        return services;
    }
}