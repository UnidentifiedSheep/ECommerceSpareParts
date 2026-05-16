using System.Reflection;
using Api.Common;
using Api.Common.Extensions;
using Api.Common.Middleware;
using Api.Common.Models;
using Api.Common.Models.Options;
using Api.Common.OperationFilters;
using Cache;
using Carter;
using Common;
using Contracts.Currency;
using Contracts.Markup;
using Contracts.Settings;
using Internal.Integration.Di;
using Localization.Abstractions.Models;
using Localization.Domain.Extensions;
using Localization.Domain.Middlewares;
using MassTransit;
using Microsoft.AspNetCore.HttpOverrides;
using OpenTelemetry.Metrics;
using Persistence.Extensions;
using Pricing.Application;
using Pricing.Persistence;
using Pricing.Persistence.Contexts;
using RabbitMq;
using RabbitMq.Extensions;
using Security;

var builder = WebApplication.CreateBuilder(args);

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";

builder.Configuration
    .AddAppSettingsFromJsons(env)
    .AddAppSettingsFromJsons(env, "/app/configs")
    .AddConfigsFromJsons("pricing", env, "/app/configs");

builder.Host.AddLokiLogger(
    configuration: builder.Configuration, 
    serviceName: "pricing.api", 
    environment: env);

builder.Services.AddMessageBrokerOptions()
    .AddHeaderSecretsOptions()
    .AddRedisOptions()
    .AddDatabaseOptions();

builder.Services.AddHttpContextAccessor();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.OperationFilter<PermissionsOperationFilter>(); });

var uniqQueueName = $"queue-of-pricing-{Environment.MachineName}";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetAssembly(typeof(Global)));

    x.AddEntityFrameworkOutbox<DContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureRabbitMq(context);

        cfg.ReceiveEndpoint(uniqQueueName, ep =>
        {
            ep.AutoDelete = true;
            ep.Durable = false;

            ep.ConfigureConsumeTopology = false;

            ep.Bind<CurrencyRateChangedEvent>();
            ep.Bind<SettingChangedEvent>();
            ep.Bind<MarkupGroupChangedEvent>();
            ep.Bind<MarkupGroupGeneratedEvent>();
            ep.Bind<MarkupRangesUpdatedEvent>();
        });

        cfg.ReceiveEndpoint("pricing-queue", ep =>
        {
            ep.Durable = true;
        });
    });
});

builder.Services
    .AddEComAuth(builder.Configuration)
    .AddPersistenceLayer()
    .AddCacheLayer("pricing")
    .AddJsonSigner(
        builder.Configuration["SignSecret"] 
        ?? throw new InvalidOperationException("Unable to find SignSecret"), 
        Global.JsonOptions)
    .AddMinimalSecurityLayer()
    .AddIntegrationClients()
    .AddCommonLayer()
    .AddApplicationLayer()
    .AddLocalization(builder.Configuration);

builder.Services.AddBaseExceptionHandlers();

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddProcessInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    });

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

var endpointAssembly = typeof(Program).Assembly;
builder.Services.AddCarter(
    new DependencyContextAssemblyCatalog(endpointAssembly),
    configurator: c => c.WithEmptyValidators());

builder.Services.AddTransient<HeaderSecretMiddleware>();

var app = builder.Build();

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

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapHealthChecks("/health");

await app.RunAsync();
