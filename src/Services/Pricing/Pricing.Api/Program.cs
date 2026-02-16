using System.Reflection;
using Abstractions.Interfaces.Currency;
using Api.Common;
using Api.Common.ExceptionHandlers;
using Api.Common.Logging;
using Api.Common.Middleware;
using Api.Common.OperationFilters;
using Application.Common.Interfaces.Settings;
using Carter;
using Contracts.Currency;
using Contracts.Currency.GetCurrencies;
using Contracts.Markup;
using Contracts.Settings;
using Hangfire;
using Microsoft.AspNetCore.HttpOverrides;
using OpenTelemetry.Metrics;
using MassTransit;
using Persistence.Extensions;
using Pricing.Abstractions.Constants;
using Pricing.Api.EndPoints.Prices;
using Pricing.Application;
using Pricing.Application.Consumers;
using Pricing.Cache;
using Pricing.Persistence;
using Pricing.Persistence.Contexts;
using RabbitMq.Extensions;
using RabbitMq.Models;
using Redis;
using Security;
using Security.Utils;
using Serilog;
using Serilog.Sinks.Loki;
using Serilog.Sinks.Loki.Labels;

var builder = WebApplication.CreateBuilder(args);

var certsPath = Environment.GetEnvironmentVariable("CERTS_PATH");
if (!string.IsNullOrWhiteSpace(certsPath))
    Certs.RegisterCerts(certsPath);

var lokiUrl = Environment.GetEnvironmentVariable("LOKI_URL");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Conditional(
        _ => !string.IsNullOrWhiteSpace(lokiUrl),
        wt => wt.LokiHttp(() => new LokiSinkConfiguration
        {
            LokiUrl = lokiUrl!,
            LogLabelProvider = new CustomLogLabelProvider([
                new LokiLabel("service", "pricing.api"),
                new LokiLabel(
                    "env",
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown"
                ),
            ])
        })
    )
    .CreateLogger();


builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();


builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<PermissionsOperationFilter>();
});

var brokerOptions = new MessageBrokerOptions
{
    Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST")!,
    Username = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER")!,
    Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS")!
};
builder.Services.AddSingleton(brokerOptions);

var uniqQueueName = $"queue-of-pricing-{Environment.MachineName}";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetAssembly(typeof(Global)));
    
    x.AddEntityFrameworkOutbox<DContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.AddRequestClient<GetCurrenciesRequest>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureRabbitMq(brokerOptions);
        
        cfg.ReceiveEndpoint(uniqQueueName, ep =>
        {
            ep.AutoDelete = true;
            ep.Durable = false;
            
            ep.ConfigureConsumeTopology = false;
            
            ep.ConfigureConsumer<SettingChangedConsumer>(context);
            ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
            ep.ConfigureConsumer<MarkupGroupChangedConsumer>(context);
            ep.ConfigureConsumer<MarkupGroupGeneratedConsumer>(context);
            ep.ConfigureConsumer<MarkupRangesChangedConsumer>(context);
            
            ep.Bind<CurrencyRateChangedEvent>();
            ep.Bind<SettingChangedEvent>();
            ep.Bind<MarkupGroupChangedEvent>();
            ep.Bind<MarkupGroupGeneratedEvent>();
            ep.Bind<MarkupRangesUpdatedEvent>();
        });
        
        cfg.ReceiveEndpoint("pricing-queue", ep =>
        {
            ep.Durable = true;
            ep.ConfigureConsumer<ArticleBuyPricesChangedConsumer>(context);
            ep.ConfigureConsumer<UserDiscountChangedConsumer>(context);
        });
    });
});

builder.Services
    .AddPersistenceLayer(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!)
    .AddCacheLayer(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")!, "pricing")
    .AddAppCacheLayer()
    .AddSecurityLayer(Environment.GetEnvironmentVariable("SIGN_SECRET")!, Global.JsonOptions)
    .AddApplicationLayer()
    .AddCommonLayer();




builder.Services.AddExceptionHandler<CustomExceptionHandler>();

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

var endpointAssembly = typeof(GetPricesEndPoint).Assembly;
builder.Services.AddCarter(new DependencyContextAssemblyCatalog(endpointAssembly));
var secret = Environment.GetEnvironmentVariable("GATEWAY_SUPER_KEY")!;
builder.Services.AddTransient<HeaderSecretMiddleware>(_ => new HeaderSecretMiddleware(secret));

var app = builder.Build();

Pricing.Application.Configs.Mapster.Configure();

app.UseMiddleware<HeaderSecretMiddleware>();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (Environment.GetEnvironmentVariable("USE_HTTPS_REDIRECTION") == "true")
    app.UseHttpsRedirection();

if (Environment.GetEnvironmentVariable("SEED_DB") == "true")
    await app.SeedAsync<DContext>();

app.UseExceptionHandler(_ => { });

app.UseRouting();

app.UseCors();


app.MapCarter();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

await InitSettings(app.Services);
await SetupCurrencies(app.Services);

await app.RunAsync();


return;

async Task SetupCurrencies(IServiceProvider serviceProvider)
{
    var busControl = app.Services.GetRequiredService<IBusControl>();
    await busControl.StartAsync();
    using var scope = serviceProvider.CreateScope();
    var currencyConverterSetup = scope.ServiceProvider.GetRequiredService<ICurrencyConverterSetup>();
    await currencyConverterSetup.InitializeAsync();
}

async Task InitSettings(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var sr = scope.ServiceProvider.GetRequiredService<ISettingsService>();
    await sr.LoadAsync(Settings.AllSettings);
}