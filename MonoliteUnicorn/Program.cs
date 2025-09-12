using System.Text;
using Api.Common;
using Api.Common.BackgroundServices;
using Api.Common.ExceptionHandlers;
using Api.Common.HangFireTasks;
using Api.Common.Logging;
using Application.Configs;
using Carter;
using Core.Interfaces;
using Core.Models;
using Hangfire;
using Hangfire.PostgreSql;
using Integrations;
using Mail;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using Persistence;
using Persistence.Contexts;
using Persistence.Entities;
using RabbitMq;
using Security;
using Serilog;
using Serilog.Sinks.Loki;
using Serilog.Sinks.Loki.Labels;

using ApplicationServiceProvider = Application.ServiceProvider;
using CacheServiceProvider = Redis.ServiceProvider;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.LokiHttp(() => new LokiSinkConfiguration
    {
        LokiUrl = builder.Configuration["Loki:Url"],
        LogLabelProvider = new CustomLogLabelProvider([new LokiLabel("app", "app"), new LokiLabel("monolite-unicorn", "monolite-unicorn")]),
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
    Password = builder.Configuration["RabbitMqSettings:Password"]!,
};

ApplicationServiceProvider.AddApplicationLayer(builder.Services)
    .AddPersistenceLayer(builder.Configuration["ConnectionStrings:DefaultConnection"]!)
    .AddMassageBrokerLayer(brokerOptions);
CacheServiceProvider.AddCacheLayer(builder.Services, builder.Configuration["ConnectionStrings:RedisConnection"]!)
    .AddSecurityLayer()
    .AddMailLayer()
    .AddCommonLayer()
    .AddIntegrations(builder.Configuration);

builder.Services.AddIdentity<UserModel, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
    }).AddEntityFrameworkStores<IdentityContext>()
    .AddUserManager<UserManager<UserModel>>()
    .AddRoleManager<RoleManager<IdentityRole>>()
    .AddSignInManager<SignInManager<UserModel>>()
    .AddDefaultTokenProviders();


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AMW", policy =>
    {
        policy.RequireRole("ADMIN", "MODERATOR", "WORKER");
    });
    options.AddPolicy("AM", policy =>
    {
        policy.RequireRole("ADMIN", "MODERATOR");
    });
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtBearer:IssuerSigningKey"]!))
    };
});
builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build());

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


var app = builder.Build();

app.UseHangfireDashboard();

MapsterConfig.Configure();
SortByConfig.Configure();

await SetupPrice(app.Services);

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
RecurringJob.AddOrUpdate<UpdateMarkUp>("UpdateMarkUp", 
    x => x.Run(), Cron.Weekly);

app.UseOpenTelemetryPrometheusScrapingEndpoint();


app.Run();
return;

async Task SetupPrice(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var priceSetup = scope.ServiceProvider.GetRequiredService<IPriceSetup>();
    await priceSetup.SetupAsync();
}
