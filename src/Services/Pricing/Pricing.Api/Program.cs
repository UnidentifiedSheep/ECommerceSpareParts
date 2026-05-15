using System.Reflection;
using Api.Common;
using Api.Common.Extensions;
using Api.Common.Middleware;
using Api.Common.Models;
using Api.Common.OperationFilters;
using Cache;
using Carter;
using Common;
using Contracts.Currency;
using Contracts.Markup;
using Contracts.Settings;
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
using RabbitMq.Extensions;
using RabbitMq.Models;
using Security;

var builder = WebApplication.CreateBuilder(args);

var lokiUrl = Environment.GetEnvironmentVariable("LOKI_URL");
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";

builder.Configuration
    .AddAppSettingsFromJsons(env)
    .AddAppSettingsFromJsons(env, "/app/configs");

builder.Host.AddLokiLogger(builder.Configuration, "pricing.api", env, lokiUrl);

builder.Services.AddHttpContextAccessor();


builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.OperationFilter<PermissionsOperationFilter>(); });

builder.Services.AddOptions<HeaderSecretOptions>()
    .BindConfiguration(HeaderSecretOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<MessageBrokerOptions>()
    .BindConfiguration(MessageBrokerOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

var brokerOptions = builder.Configuration
                        .GetSection(MessageBrokerOptions.SectionName)
                        .Get<MessageBrokerOptions>()
                    ?? throw new NullReferenceException(
                        $"Missing {MessageBrokerOptions.SectionName} configuration options");

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
        cfg.ConfigureRabbitMq(brokerOptions);

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

Locale[] locales = ["ru-RU", "en-EN"];
Locale defaultLocale = "ru-RU";

builder.Services
    .AddPersistenceLayer(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!)
    .AddCacheLayer(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")!, "pricing")
    .AddJsonSigner(Environment.GetEnvironmentVariable("SIGN_SECRET")!, Global.JsonOptions)
    .AddMinimalSecurityLayer()
    .AddCommonLayer()
    .AddApplicationLayer()
    .AddLocalization(defaultLocale, locales);

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


if (Environment.GetEnvironmentVariable("SEED_DB") == "true")
    await app.SeedAsync<DContext>();

app.UseExceptionHandler(_ => { });

app.UseRouting();

app.UseCors();


app.MapCarter();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapHealthChecks("/health");

await app.RunAsync();
