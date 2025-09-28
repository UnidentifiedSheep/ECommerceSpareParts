using Application.Behaviors;
using Application.ConcurrencyValidator;
using Application.Handlers.Articles.GetArticleCrosses;
using Application.Handlers.Articles.GetArticles;
using Application.Interfaces;
using Application.Pricing;
using Application.RelatedData;
using Application.Services;
using Application.Validators;
using Core.Abstractions;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.CacheRepositories;
using Core.Interfaces.Services;
using Core.Interfaces.Validators;
using Core.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using AmwArticleDto = Core.Dtos.Amw.Articles.ArticleDto;
using AnonymousArticleDto = Core.Dtos.Anonymous.Articles.ArticleDto;
using AmwArticleFullDto = Core.Dtos.Amw.Articles.ArticleFullDto;
using MemberArticleFullDto = Core.Dtos.Member.Articles.ArticleFullDto;

namespace Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection collection, 
        UserEmailOptions? emailOptions = null, UserPhoneOptions? phoneOptions = null)
    {
        var relatedDataTtl = TimeSpan.FromHours(10);
        collection.AddSingleton(emailOptions ?? new UserEmailOptions());
        collection.AddSingleton(phoneOptions ?? new UserPhoneOptions());
        collection.AddSingleton<ICurrencyConverter, CurrencyConverter>(_ => new CurrencyConverter(Global.UsdId));
        collection.AddSingleton<IPriceGenerator, PriceGenerator>();
        collection.AddScoped<IMarkupGenerator, MarkupGenerator>();
        collection.AddScoped<IPriceSetup, PriceSetup>();

        collection.AddScoped<IArticlePricesService, ArticlePricesService>();
        collection.AddScoped<IArticlesService, ArticlesService>();
        collection.AddScoped<IBalanceService, BalanceService>();
        collection.AddScoped<ISaleService, SaleService>();
        collection.AddScoped<IUserTokenService, UserTokenService>();

        collection.AddSingleton<IEmailValidator, EmailValidator>();
        collection.AddSingleton<IConcurrencyValidator<StorageContent>, StorageContentConcurrencyValidator>();
        
        collection.AddTransient<RelatedDataBase<Article>, ArticleRelatedData>(sp =>
        {
            var cache = sp.GetRequiredService<ICache>();
            return new ArticleRelatedData(cache, relatedDataTtl);
        });
        collection.AddTransient<RelatedDataBase<Producer>, ProducerRelatedData>(sp =>
        {
            var cache = sp.GetRequiredService<ICache>();
            return new ProducerRelatedData(cache, relatedDataTtl);
        });
        
        collection.AddTransient<RelatedDataBase<Currency>, CurrencyRelatedData>(sp =>
        {
            var cache = sp.GetRequiredService<ICache>();
            return new CurrencyRelatedData(cache, relatedDataTtl);
        });

        collection.AddValidatorsFromAssembly(typeof(Global).Assembly);

        collection
            .AddScoped<IRequestHandler<GetArticlesQuery<AmwArticleDto>, GetArticlesResult<AmwArticleDto>>,
                GetArticlesHandler<AmwArticleDto>>();
        collection
            .AddScoped<IRequestHandler<GetArticlesQuery<AnonymousArticleDto>, GetArticlesResult<AnonymousArticleDto>>,
                GetArticlesHandler<AnonymousArticleDto>>();
        collection
            .AddScoped<IRequestHandler<GetArticleCrossesQuery<AmwArticleFullDto>,
                GetArticleCrossesResult<AmwArticleFullDto>>, GetArticleCrossesHandler<AmwArticleFullDto>>();
        collection
            .AddScoped<IRequestHandler<GetArticleCrossesQuery<MemberArticleFullDto>,
                GetArticleCrossesResult<MemberArticleFullDto>>, GetArticleCrossesHandler<MemberArticleFullDto>>();

        collection.Scan(scan => scan
            .FromAssemblyOf<GetArticlesAmwLogSettings>()
            .AddClasses(classes => classes.Where(type =>
                type.GetInterfaces()
                    .Any(i => i.IsGenericType &&
                              i.GetGenericTypeDefinition() == typeof(ILoggableRequest<>))))
            .As(type => type.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(ILoggableRequest<>)))
            .WithScopedLifetime()
        );

        /*collection.Scan(scan => scan
            .FromAssemblyOf<GetArticlesAmwLogSettings>()
            .AddClasses(classes => classes.Where(type =>
                type.GetInterfaces()
                    .Any(i => i.IsGenericType &&
                              i.GetGenericTypeDefinition() == typeof(ICacheableQuery<>))))
            .As(type => type.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(ICacheableQuery<>)))
            .WithScopedLifetime()
        );*/


        collection.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(Global).Assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(RequestsDataLoggingBehavior<,>));
            config.AddOpenBehavior(typeof(CacheBehavior<,>));
            config.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        return collection;
    }
}