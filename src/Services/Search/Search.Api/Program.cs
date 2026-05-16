using System.Reflection;
using Abstractions.Interfaces.HostedServices;
using Api.Common;
using Api.Common.Extensions;
using Api.Common.HostedServices;
using Api.Common.Middleware;
using Api.Common.OperationFilters;
using Carter;
using Common;
using Localization.Domain.Extensions;
using Localization.Domain.Middlewares;
using MassTransit;
using Microsoft.AspNetCore.HttpOverrides;
using RabbitMq.Extensions;
using Search.Api.EndPoints.Articles;
using Search.Application;
using Search.Persistence;
using Security;

var builder = WebApplication.CreateBuilder(args);
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";

builder.Configuration
    .AddAppSettingsFromJsons(env)
    .AddAppSettingsFromJsons(env, "/app/configs")
    .AddConfigsFromJsons("search", env, "/app/configs");

builder.Host.AddLokiLogger(
    builder.Configuration, 
    "search.api", 
    env);

builder.Services.AddMessageBrokerOptions()
    .AddHeaderSecretsOptions()
    .AddRedisOptions();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.OperationFilter<PermissionsOperationFilter>(); });

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureRabbitMq(context);

        cfg.ReceiveEndpoint("search-queue", ep =>
        {
            ep.Durable = true;

        });
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<BackgroundTaskQueue>();
builder.Services.AddSingleton<IBackgroundTaskQueue>(sp => sp.GetRequiredService<BackgroundTaskQueue>());
builder.Services.AddHostedService<BackgroundTaskQueue>(sp => sp.GetRequiredService<BackgroundTaskQueue>());

builder.Services.AddPersistenceLayer(builder.Configuration.GetValue<string>("IndexPath") ?? "./data")
    .AddEComAuth(builder.Configuration)
    .AddMinimalSecurityLayer()
    .AddApplicationLayer()
    .AddLocalization(builder.Configuration)
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
builder.Services.AddCarter(
    new DependencyContextAssemblyCatalog(endpointAssembly),
    configurator: c => c.WithEmptyValidators());

builder.Services.AddTransient<HeaderSecretMiddleware>();

var app = builder.Build();

await app.LoadLocalesFromJson(
    Assembly.GetExecutingAssembly().GetDefaultLocalizationPath());

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
