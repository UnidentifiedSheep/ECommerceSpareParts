using System.Reflection;
using System.Text;
using Abstractions.Interfaces.Currency;
using Abstractions.Models;
using Amazon.S3;
using Api.Common;
using Api.Common.ExceptionHandlers;
using Api.Common.Logging;
using Api.Common.Middleware;
using Api.Common.OperationFilters;
using Application.Common.Interfaces.Settings;
using Carter;
using Contracts.Currency;
using Contracts.Settings;
using ExchangeRate;
using Hangfire;
using Hangfire.PostgreSql;
using Mail;
using Main.Abstractions.Constants;
using Main.Api.EndPoints.Articles;
using Main.Application;
using Main.Application.BackgroundServices;
using Main.Application.Configs;
using Main.Application.Consumers;
using Main.Application.HangFireTasks;
using Main.Application.Seeding;
using Main.Cache;
using Microsoft.AspNetCore.HttpOverrides;
using OpenTelemetry.Metrics;
using Main.Persistence;
using Main.Persistence.Context;
using MassTransit;
using Persistence.Extensions;
using RabbitMq.Extensions;
using RabbitMq.Models;
using Redis;
using S3;
using Security;
using Security.Utils;
using Serilog;
using Serilog.Sinks.Loki;
using Serilog.Sinks.Loki.Labels;
using Global = Main.Application.Global;

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
                new LokiLabel("service", "main.api"),
                new LokiLabel(
                    "env",
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown"
                ),
            ])
        })
    )
    .CreateLogger();


builder.Host.UseSerilog();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<PermissionsOperationFilter>();
});

builder.Services.AddHangfire(x =>
    x.UsePostgreSqlStorage(z => 
        z.UseNpgsqlConnection(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"))));
builder.Services.AddHangfireServer();

var brokerOptions = new MessageBrokerOptions
{
    Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST")!,
    Username = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER")!,
    Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS")!
};
builder.Services.AddSingleton(brokerOptions);

var emailOptions = new UserEmailOptions
{
    MinEmailCount = 1,
    MaxEmailCount = 5
};

var uniqQueueName = $"queue-of-main-{Environment.MachineName}";

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
            
            ep.ConfigureConsumer<SettingChangedConsumer>(context);
            ep.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
            
            ep.Bind<CurrencyRateChangedEvent>();
            ep.Bind<SettingChangedEvent>();
        });
        
        cfg.ReceiveEndpoint("main-queue", ep =>
        {
            ep.Durable = true;
            
            ep.ConfigureConsumer<GetArticleCoefficientsConsumer>(context);
            ep.ConfigureConsumer<GetCurrenciesConsumer>(context);
            ep.ConfigureConsumer<GetStorageContentCostsConsumer>(context);
        });
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddPersistenceLayer(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!)
    .AddCacheLayer(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")!, "main")
    .AddAppCacheLayer()
    .AddSecurityLayer(Environment.GetEnvironmentVariable("SIGN_SECRET")!, Global.JsonOptions)
    .AddMailLayer()
    .AddCommonLayer()
    .AddS3(() =>
    {
        var config = new AmazonS3Config
        {
            ServiceURL = Environment.GetEnvironmentVariable("S3_URL"),
            ForcePathStyle = Environment.GetEnvironmentVariable("S3_FORCE_PATH_STYLE") == "true",
        };
        return new AmazonS3Client(Environment.GetEnvironmentVariable("S3_LOGIN"), 
            Environment.GetEnvironmentVariable("S3_PASSWORD"), config);
    })
    .AddApplicationLayer(emailOptions)
    .AddExchangeRates();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddHostedService<SearchLogBackgroundWorker>();

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

var endpointAssembly = typeof(AddArticleContentEndPoint).Assembly;
builder.Services.AddCarter(new DependencyContextAssemblyCatalog(endpointAssembly));
var secret = Environment.GetEnvironmentVariable("GATEWAY_SUPER_KEY")!;
builder.Services.AddTransient<HeaderSecretMiddleware>(_ => new HeaderSecretMiddleware(secret));

var app = builder.Build();

MapsterConfig.Configure();
SortByConfig.Configure();

if (Environment.GetEnvironmentVariable("SEED_DB") == "true")
    await app.SeedAsync<DContext>();

if (Environment.GetEnvironmentVariable("SEED_ADMIN") == "true")
{
    var login = Environment.GetEnvironmentVariable("SEED_ADMIN_LOGIN");
    if (string.IsNullOrWhiteSpace(login)) login = "Administrator";
    var password = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD");
    if (string.IsNullOrWhiteSpace(password)) password = "Administrator12345"; 
    var email = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL");
    if (string.IsNullOrWhiteSpace(email)) email = "emailNotProvided@some.com";
    
    await UserSeed.SeedAdmin(login, password, email, app.Services);
}
    
    
app.UseMiddleware<HeaderSecretMiddleware>();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

Global.SetSystemId(Environment.GetEnvironmentVariable("SYSTEM_ID")!);
Global.SetServiceUrl(Environment.GetEnvironmentVariable("S3_SERVICE_URL")!);
Global.SetImageBucketName(Environment.GetEnvironmentVariable("S3_IMAGES_BUCKET")!);

app.UseHangfireDashboard();

await InitCurrencyConverter(app.Services);
await InitSettings(app.Services);

if (Environment.GetEnvironmentVariable("USE_HTTPS_REDIRECTION") == "true")
    app.UseHttpsRedirection();

app.UseExceptionHandler(_ => { });

app.UseRouting();

app.UseCors();


app.MapCarter();

if (app.Environment.IsDevelopment())
{
    app.UseReDoc(options =>
    {
        options.DocumentTitle = "Main API Docs";
        options.SpecUrl = "/swagger/v1/swagger.json";
        options.RoutePrefix = "docs";
    });
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}


RecurringJob.AddOrUpdate<UpdateCurrencyRate>("UpdateCurrencyTask",
    x => x.Run(), Cron.Daily);

RecurringJob.AddOrUpdate<NotifySuggestionsRebuildNeeded>("UpdateCurrencyTask",
    x => x.Run(), Cron.Daily);

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();


return;

async Task InitCurrencyConverter(IServiceProvider serviceProvider)
{
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