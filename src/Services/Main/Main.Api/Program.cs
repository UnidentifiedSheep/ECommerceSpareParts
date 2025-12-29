using System.Text;
using Amazon.S3;
using Api.Common;
using Api.Common.ExceptionHandlers;
using Api.Common.Logging;
using Api.Common.Middleware;
using Api.Common.OperationFilters;
using Api.Common.SchemaFilters;
using Carter;
using Contracts.Currency;
using Contracts.Markup;
using Core.Interfaces.MessageBroker;
using Core.Models;
using Core.StaticFunctions;
using Hangfire;
using Hangfire.PostgreSql;
using Integrations;
using Mail;
using Main.Api.EndPoints.Articles;
using Main.Application;
using Main.Application.BackgroundServices;
using Main.Application.Configs;
using Main.Application.EventHandlers;
using Main.Application.HangFireTasks;
using Main.Application.Seeding;
using Main.Core.Interfaces.Pricing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using Main.Persistence;
using Main.Persistence.Context;
using MassTransit;
using Persistence.Extensions;
using RabbitMq;
using Redis;
using S3;
using Security;
using Serilog;
using Serilog.Sinks.Loki;
using Serilog.Sinks.Loki.Labels;
using Global = Main.Application.Global;

var builder = WebApplication.CreateBuilder(args);

var certsPath = Environment.GetEnvironmentVariable("CERTS_PATH");
if (!string.IsNullOrWhiteSpace(certsPath))
    Certs.RegisterCerts(certsPath);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.LokiHttp(() => new LokiSinkConfiguration
    {
        LokiUrl = builder.Configuration["Loki:Url"],
        LogLabelProvider = new CustomLogLabelProvider([
            new LokiLabel("app", "app"), new LokiLabel("monolite-unicorn", "monolite-unicorn")
        ])
    })
    .WriteTo.Console()
    .CreateLogger();


builder.Host.UseSerilog();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<PermissionsOperationFilter>();
    c.SchemaFilter<ExceptionToProblemFilter>();
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
    MinEmailCount = 0,
    MaxEmailCount = 5
};

var uniqQueueName = $"queue-of-main-{Environment.MachineName}";

ConsumerRegistration[] eventHandlers =
[
    new(typeof(MarkupGroupChangedEvent), uniqQueueName),
    new(typeof(MarkupRangesUpdatedEvent), uniqQueueName),
    new(typeof(CurrencyRateChangedEvent), uniqQueueName),
    new(typeof(MarkupGroupGeneratedEvent), "main-queue")
];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var iss = builder.Configuration["JwtBearer:ValidIssuer"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = iss,
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtBearer:IssuerSigningKey"]!)),
    };
});

builder.Services.AddScoped<IEventHandler<MarkupGroupChangedEvent>, MarkupGroupChangedEventHandler>();
builder.Services.AddScoped<IEventHandler<MarkupRangesUpdatedEvent>, MarkupRangesChangedEventHandler>();
builder.Services.AddScoped<IEventHandler<CurrencyRateChangedEvent>, CurrencyRatesChangedEventHandler>();
builder.Services.AddScoped<IEventHandler<MarkupGroupGeneratedEvent>, MarkupGroupGeneratedEventHandler>();

builder.Services
    .AddPersistenceLayer(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!)
    .AddCacheLayer(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")!)
    .AddSecurityLayer()
    .AddMailLayer()
    .AddMassageBrokerLayer<DContext>(brokerOptions, eventHandlers,
        opt =>
        {
            opt.UseBusOutbox();
            opt.UsePostgres();
        })
    .AddCommonLayer()
    .AddIntegrations(builder.Configuration)
    .AddS3(() =>
    {
        var config = new AmazonS3Config
        {
            ServiceURL = Environment.GetEnvironmentVariable("S3_SERVICE_URL"),
            ForcePathStyle = Environment.GetEnvironmentVariable("S3_FORCE_PATH_STYLE") == "true",
        };
        return new AmazonS3Client(Environment.GetEnvironmentVariable("S3_LOGIN"), 
            Environment.GetEnvironmentVariable("S3_PASSWORD"), config);
    })
    .AddApplicationLayer(emailOptions);





builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser()
        .Build());


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
    
    await UserSeed.SeedAdmin(login, password, app.Services);
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

await SetupPrice(app.Services);
app.UseHttpsRedirection();

app.UseExceptionHandler(_ => { });

app.UseAuthentication();
app.UseAuthorization();

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

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();


return;

async Task SetupPrice(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var priceSetup = scope.ServiceProvider.GetRequiredService<IPriceSetup>();
    await priceSetup.SetupAsync();
}