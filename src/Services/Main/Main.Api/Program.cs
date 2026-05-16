using System.Reflection;
using Abstractions.Models;
using Amazon.S3;
using Api.Common;
using Api.Common.Extensions;
using Api.Common.Middleware;
using Api.Common.Models;
using Api.Common.Models.Options;
using Api.Common.OperationFilters;
using Application.Common.Interfaces.Settings;
using Cache;
using Carter;
using Common;
using Contracts.Currency;
using Contracts.Settings;
using ExchangeRate;
using Hangfire;
using Hangfire.PostgreSql;
using Localization.Domain.Extensions;
using Localization.Domain.Middlewares;
using Mail;
using Main.Api.EndPoints.Products;
using Main.Application;
using Main.Application.BackgroundServices;
using Main.Application.Configs;
using Main.Application.Consumers;
using Main.Application.HangFireTasks;
using Main.Application.Models;
using Main.Cache;
using Main.Persistence;
using Main.Persistence.Context;
using MassTransit;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using Persistence;
using RabbitMq;
using RabbitMq.Extensions;
using S3;
using Security;
using Global = Main.Application.Global;

var builder = WebApplication.CreateBuilder(args);

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";

builder.Configuration
    .AddAppSettingsFromJsons(env)
    .AddAppSettingsFromJsons(env, "/app/configs")
    .AddConfigsFromJsons("main", env, "/app/configs");

builder.Host.AddLokiLogger(
    configuration: builder.Configuration, 
    serviceName: "main.api", 
    environment: env);

builder.Services.AddMessageBrokerOptions()
    .AddHeaderSecretsOptions()
    .AddRedisOptions()
    .AddS3Options()
    .AddDatabaseOptions();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.OperationFilter<PermissionsOperationFilter>(); });

builder.Services.AddHangfire((sp, x) =>
{
    var options = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    x.UsePostgreSqlStorage(z =>
        z.UseNpgsqlConnection(options.ConnectionString));
});

builder.Services.AddHangfireServer();

var emailOptions = new UserEmailOptions
{
    MinEmailCount = 1,
    MaxEmailCount = 5
};

//looks bas asf
builder.Services.AddSingleton(new JwtOptions(
    builder.Configuration["JwtBearer:IssuerSigningKey"]!,
    builder.Configuration["JwtBearer:ValidIssuer"]!,
    TimeSpan.FromMilliseconds(builder.Configuration.GetValue<long>("JwtBearer:ValidDurationMs"))));

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
        cfg.ConfigureRabbitMq(context);

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

            ep.ConfigureConsumer<CurrencyCreatedConsumer>(context);
            ep.ConfigureConsumer<ProductSizesUpdatedConsumer>(context);
            ep.ConfigureConsumer<ProductWeightUpdatedConsumer>(context);
            ep.ConfigureConsumer<ProductUpdatedConsumer>(context);
            ep.ConfigureConsumer<RoleUpdatedConsumer>(context);
            ep.ConfigureConsumer<UserUpdatedConsumer>(context);
        });
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddPersistenceLayer()
    .AddCacheLayer("main")
    .AddApplicationCache()
    .AddJsonSigner(
        builder.Configuration["SignSecret"] ??
        throw new InvalidOperationException("SignSecret not found in configuration"), 
        Global.JsonOptions)
    .AddFullSecurityLayer()
    .AddEComAuth(builder.Configuration)
    .AddMailLayer()
    .AddCommonLayer()
    .AddS3(sp =>
    {
        var options = sp.GetRequiredService<IOptions<S3Options>>().Value;
        var config = new AmazonS3Config
        {
            ServiceURL = options.Url,
            ForcePathStyle = options.ForcePathStyle
        };
        return new AmazonS3Client(options.Login, options.Password, config);
    })
    .AddApplicationLayer(emailOptions)
    .AddLocalization(builder.Configuration)
    .AddExchangeRates();

builder.Services.AddBaseExceptionHandlers();

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

builder.Services.AddCarter(
    new DependencyContextAssemblyCatalog(typeof(AddProductContentEndPoint).Assembly),
    configurator: c => c.WithEmptyValidators());

builder.Services.AddTransient<HeaderSecretMiddleware>();

var app = builder.Build();

SortByConfig.Configure();

app.UseMiddleware<HeaderSecretMiddleware>();

app.UseMiddleware<ScopedLocalizationMiddleware>();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHangfireDashboard();

var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
await app.LoadLocalesFromJson(localesPath);
await InitSettings(app.Services);

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

RecurringJob.AddOrUpdate<NotifySuggestionsRebuildNeeded>("RebuildSuggestionsTask",
    x => x.Run(), Cron.Daily);

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapHealthChecks("/health");

await app.RunAsync();


return;

async Task InitSettings(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var sr = scope.ServiceProvider.GetRequiredService<ISettingsService>();
    await sr.LoadAsync();
}
