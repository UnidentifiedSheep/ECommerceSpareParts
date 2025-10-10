using System.Text;
using Api.Common;
using Api.Common.ExceptionHandlers;
using Api.Common.Logging;
using Carter;
using Contracts;
using Core.Interfaces.MessageBroker;
using Core.Models;
using Hangfire;
using Hangfire.PostgreSql;
using Integrations;
using Mail;
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

var mainQueueName = $"queue-of-main-{Environment.MachineName}";

ConsumerRegistration[] eventHandlers =
[
    new(typeof(MarkupGroupChangedEvent), mainQueueName),
    new(typeof(MarkupRangesUpdatedEvent), mainQueueName),
    new(typeof(CurrencyRateChangedEvent), mainQueueName)
];

builder.Services.AddScoped<IEventHandler<MarkupGroupChangedEvent>, MarkupGroupChangedEventHandler>();
builder.Services.AddScoped<IEventHandler<MarkupRangesUpdatedEvent>, MarkupRangesChangedEventHandler>();
builder.Services.AddScoped<IEventHandler<CurrencyRateChangedEvent>, CurrencyRatesChangedEventHandler>();

builder.Services.AddApplicationLayer(emailOptions)
    .AddPersistenceLayer(builder.Configuration["ConnectionStrings:DefaultConnection"]!)
    .AddMassageBrokerLayer(brokerOptions, eventHandlers)
    .AddCacheLayer(builder.Configuration["ConnectionStrings:RedisConnection"]!)
    .AddSecurityLayer()
    .AddMailLayer()
    .AddCommonLayer()
    .AddIntegrations(builder.Configuration);


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AMW", policy => { policy.RequireRole("Admin", "Moderator", "Worker"); });
    options.AddPolicy("AM", policy => { policy.RequireRole("Admin", "Moderator"); });
});

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
        ValidAudience = builder.Configuration["JwtBearer:ValidAudience"],
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtBearer:IssuerSigningKey"]!))
    };
});
builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser()
        .Build());

builder.Services.AddCarter();
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


var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHangfireDashboard();

MapsterConfig.Configure();
SortByConfig.Configure();

await SetupPrice(app.Services);

app.UseCors();
app.UseExceptionHandler(_ => { });
app.UseAuthentication();
app.UseAuthorization();
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