using Application.Common;
using Application.Common.Behaviors;
using Application.Common.Factories;
using Application.Common.Interfaces;
using Application.Common.Validators;
using Core.Abstractions;
using Core.Interfaces;
using Core.Interfaces.CacheRepositories;
using Core.Interfaces.Validators;
using Core.Models;
using FluentValidation;
using Main.Application.ConcurrencyValidator;
using Main.Application.Handlers.Articles.GetArticleCrosses;
using Main.Application.Handlers.Articles.GetArticles;
using Main.Application.HangFireTasks;
using Main.Application.Pricing;
using Main.Application.RelatedData;
using Main.Application.Services;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Entities;
using Main.Core.Interfaces.Pricing;
using Main.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using AmwArticleDto = Main.Core.Dtos.Amw.Articles.ArticleDto;
using AnonymousArticleDto = Main.Core.Dtos.Anonymous.Articles.ArticleDto;
using AmwArticleFullDto = Main.Core.Dtos.Amw.Articles.ArticleFullDto;
using Currency = Main.Core.Entities.Currency;
using MemberArticleFullDto = Main.Core.Dtos.Member.Articles.ArticleFullDto;

namespace Main.Application;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection collection,
        UserEmailOptions? emailOptions = null, UserPhoneOptions? phoneOptions = null)
    {
        var relatedDataTtl = TimeSpan.FromHours(10);

        collection.AddSingleton<UpdateCurrencyRate>();
        collection.AddScoped<IRelatedDataFactory, RelatedDataFactory>();
        collection.AddSingleton(emailOptions ?? new UserEmailOptions());
        collection.AddSingleton(phoneOptions ?? new UserPhoneOptions());
        collection.AddSingleton<ICurrencyConverter, CurrencyConverter>(_ => new CurrencyConverter(Global.UsdId));
        collection.AddSingleton<IPriceGenerator, PriceGenerator>();
        collection.AddScoped<IPriceSetup, PriceSetup>();
        collection.AddScoped<DbDataValidatorBase, DbDataValidator>();
        
        collection.AddScoped<IArticlePricesService, ArticlePricesService>();
        collection.AddScoped<IArticlesService, ArticlesService>();
        collection.AddScoped<IBalanceService, BalanceService>();
        collection.AddScoped<ISaleService, SaleService>();
        collection.AddScoped<IUserTokenService, UserTokenService>();

        collection.AddSingleton<IEmailValidator, EmailValidator>();
        collection.AddSingleton<IConcurrencyValidator<StorageContent>, StorageContentConcurrencyValidator>();

        collection.AddTransient<RelatedDataBase<ArticleCross>, ArticleCrossesRelatedData>(sp =>
        {
            var cache = sp.GetRequiredService<ICache>();
            return new ArticleCrossesRelatedData(cache, relatedDataTtl);
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