using System.Reflection;
using Abstractions.Interfaces.HostedServices;
using Api.Common;
using Api.Common.Extensions;
using Api.Common.HostedServices;
using Api.Common.Middleware;
using Api.Common.OperationFilters;
using Carter;
using Localization.Abstractions.Models;
using Localization.Domain.Extensions;
using Localization.Domain.Middlewares;
using MassTransit;
using Microsoft.AspNetCore.HttpOverrides;
using RabbitMq.Extensions;
using RabbitMq.Models;
using Search.Api.EndPoints.Articles;
using Search.Application;
using Search.Application.Consumers;
using Search.Persistence;
using Security;
using Security.Utils;

var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();

var builder = WebApplication.CreateBuilder(args);

var certsPath = Environment.GetEnvironmentVariable("CERTS_PATH");
if (!string.IsNullOrWhiteSpace(certsPath))
    Certs.RegisterCerts(certsPath);

var lokiUrl = Environment.GetEnvironmentVariable("LOKI_URL");
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown";

builder.Host.AddLokiLogger(builder.Configuration, "main.api", env, lokiUrl);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.OperationFilter<PermissionsOperationFilter>(); });

var brokerOptions = new MessageBrokerOptions
{
    Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST")!,
    Username = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER")!,
    Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS")!
};

builder.Services.AddSingleton(brokerOptions);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ArticledCreatedConsumer>();
    x.AddConsumer<ArticleUpdatedConsumer>();
    x.AddConsumer<ArticleDeletedConsumer>();
    x.AddConsumer<SuggestionRebuildNeededConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureRabbitMq(brokerOptions);

        cfg.ReceiveEndpoint("search-queue", ep =>
        {
            ep.Durable = true;

            ep.ConfigureConsumer<ArticledCreatedConsumer>(context);
            ep.ConfigureConsumer<ArticleUpdatedConsumer>(context);
            ep.ConfigureConsumer<ArticleDeletedConsumer>(context);
            ep.ConfigureConsumer<SuggestionRebuildNeededConsumer>(context);
        });
    });
});

builder.Services.AddHttpContextAccessor();

Locale[] locales = ["ru-RU", "en-EN"];
Locale defaultLocale = "ru-RU";


builder.Services.AddSingleton<BackgroundTaskQueue>();
builder.Services.AddSingleton<IBackgroundTaskQueue>(sp => sp.GetRequiredService<BackgroundTaskQueue>());
builder.Services.AddHostedService<BackgroundTaskQueue>(sp => sp.GetRequiredService<BackgroundTaskQueue>());

builder.Services.AddPersistenceLayer(Environment.GetEnvironmentVariable("INDEX_FOLDER") ?? "./data")
    .AddMinimalSecurityLayer()
    .AddApplicationLayer()
    .AddLocalization(defaultLocale, locales)
    .AddBaseExceptionHandlers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var endpointAssembly = typeof(GetArticleRequest).Assembly;
builder.Services.AddCarter(new DependencyContextAssemblyCatalog(endpointAssembly));

var secret = Environment.GetEnvironmentVariable("GATEWAY_SUPER_KEY")!;
builder.Services.AddTransient<HeaderSecretMiddleware>(_ => new HeaderSecretMiddleware(secret));

var app = builder.Build();

await app.LoadLocalesFromJson(localesPath);

if (Environment.GetEnvironmentVariable("USE_HTTPS_REDIRECTION") == "true")
    app.UseHttpsRedirection();

app.UseMiddleware<HeaderSecretMiddleware>();

app.UseMiddleware<ScopedLocalizationMiddleware>();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseExceptionHandler(_ => { });

app.UseRouting();

app.UseCors();

app.MapCarter();

app.MapHealthChecks("/health");

await app.RunAsync();