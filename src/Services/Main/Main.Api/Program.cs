using System.Text;
using Api.Common;
using Api.Common.ExceptionHandlers;
using Api.Common.Logging;
using Api.Common.Middleware;
using Carter;
using Contracts;
using Contracts.Currency;
using Contracts.Markup;
using Core.Interfaces.MessageBroker;
using Core.Models;
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
using Main.Core.Interfaces.Pricing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using Main.Persistence;
using Main.Persistence.Context;
using MassTransit;
using RabbitMq;
using Redis;
using Security;
using Serilog;
using Serilog.Sinks.Loki;
using Serilog.Sinks.Loki.Labels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

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
    // Security (JWT)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

builder.Services.AddHangfire(x =>
    x.UsePostgreSqlStorage(z => z.UseNpgsqlConnection(builder.Configuration
        .GetConnectionString("DefaultConnection"))));
builder.Services.AddHangfireServer();

builder.Services.Configure<MessageBrokerOptions>(builder.Configuration.GetSection("RabbitMqSettings"));
var brokerOptions = new MessageBrokerOptions
{
    Host = builder.Configuration["RabbitMqSettings:Host"]!,
    Username = builder.Configuration["RabbitMqSettings:Username"]!,
    Password = builder.Configuration["RabbitMqSettings:Password"]!
};

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

builder.Services.AddScoped<IEventHandler<MarkupGroupChangedEvent>, MarkupGroupChangedEventHandler>();
builder.Services.AddScoped<IEventHandler<MarkupRangesUpdatedEvent>, MarkupRangesChangedEventHandler>();
builder.Services.AddScoped<IEventHandler<CurrencyRateChangedEvent>, CurrencyRatesChangedEventHandler>();
builder.Services.AddScoped<IEventHandler<MarkupGroupGeneratedEvent>, MarkupGroupGeneratedEventHandler>();

builder.Services
    .AddPersistenceLayer(builder.Configuration["ConnectionStrings:DefaultConnection"]!)
    .AddCacheLayer(builder.Configuration["ConnectionStrings:RedisConnection"]!)
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
var secret = builder.Configuration["Gateway:Secret"]!;
builder.Services.AddTransient<HeaderSecretMiddleware>(_ => new HeaderSecretMiddleware(secret));

var app = builder.Build();

app.UseMiddleware<HeaderSecretMiddleware>();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHangfireDashboard();

MapsterConfig.Configure();
SortByConfig.Configure();

await SetupPrice(app.Services);


app.UseRouting();
app.UseCors();
app.UseExceptionHandler(_ => { });

app.MapCarter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();


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