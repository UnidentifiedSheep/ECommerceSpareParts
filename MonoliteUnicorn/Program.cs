using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Amazon.S3;
using Carter;
using Core.Behavior;
using Core.Exceptions.ExceptionHandlers;
using Core.Logger;
using Core.Mail;
using Core.Mail.Models;
using Core.Redis;
using Core.Services.S3;
using Core.Services.TimeWebCloud.Implementations;
using Core.Services.TimeWebCloud.Interfaces;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MonoliteUnicorn;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.Consumers;
using MonoliteUnicorn.HangFireTasks;
using MonoliteUnicorn.PostGres.Identity;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.RedisFunctions;
using MonoliteUnicorn.Services.ArticleReservations;
using MonoliteUnicorn.Services.Balances;
using MonoliteUnicorn.Services.CacheService;
using MonoliteUnicorn.Services.Catalogue;
using MonoliteUnicorn.Services.Inventory;
using MonoliteUnicorn.Services.JWT;
using MonoliteUnicorn.Services.Prices.Price;
using MonoliteUnicorn.Services.Prices.PriceGenerator;
using MonoliteUnicorn.Services.Purchase;
using MonoliteUnicorn.Services.Sale;
using MonoliteUnicorn.Services.SearchLog;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Sinks.Loki;
using Purchase = MonoliteUnicorn.Services.Purchase.Purchase;
using Sale = MonoliteUnicorn.Services.Sale.Sale;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.LokiHttp(() => new LokiSinkConfiguration
    {
        LokiUrl = builder.Configuration["Loki:Url"],
        LogLabelProvider = new CustomLogLableProvider(),
    })
    .CreateLogger();


builder.Host.UseSerilog();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);
    
    c.TagActionsBy(api =>
    {
        var groupName = api.ActionDescriptor.EndpointMetadata
            .OfType<ApiExplorerSettingsAttribute>()
            .FirstOrDefault()?.GroupName;
        if (groupName != null)
            return [groupName];
        return [ "Default API" ];
    });

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

builder.Services.AddHangfire(x => x.UsePostgreSqlStorage(z => z.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));
builder.Services.AddHangfireServer();

builder.Services.AddScoped<UpdateCurrencyRate>();
builder.Services.AddScoped<MarkUpGenerator>();
builder.Services.AddScoped<UpdateMarkUp>();
builder.Services.AddScoped<IPrice, Price>();
builder.Services.AddScoped<IPurchase, Purchase>();
builder.Services.AddScoped<ISale, Sale>();
builder.Services.AddScoped<IBalance, Balance>();
builder.Services.AddScoped<IInventory, Inventory>();
builder.Services.AddScoped<ICatalogue, Catalogue>();
builder.Services.AddScoped<IPurchaseOrchestrator, PurchaseOrchestrator>();
builder.Services.AddScoped<ISaleOrchestrator, SaleOrchestrator>();
builder.Services.AddScoped<IArticleReservation, ArticleReservation>();
builder.Services.AddDbContext<IdentityContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<DContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddHttpClient();
builder.Services.AddHttpClient("TimewebClient", sp =>
{
    sp.BaseAddress = new Uri(builder.Configuration.GetValue<string>("TimeWebConnect:BaseUrl")!);
    sp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
        builder.Configuration.GetValue<string>("TimeWebConnect:Token")!);
});

builder.Services.AddCarter();
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

Global.S3BucketName = builder.Configuration["S3Storage:BucketName"];
Global.ServiceUrl = builder.Configuration["S3Storage:ServiceURL"];

builder.Services.AddScoped<IS3StorageService, S3StorageService>();
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddTransient<ITimeWebMail>(sp => new TimeWebMail(sp.GetService<IHttpClientFactory>()!.CreateClient("TimewebClient")));
builder.Services.Configure<MailOptions>(builder.Configuration.GetSection("Mail"));
builder.Services.AddTransient<IMail, Mail>();
builder.Services.AddTransient<IJwtGenerator, JwtGenerator>();
builder.Services.AddTransient<IRedisArticleRepository, RedisArticleRepository>(_ =>
{
    var redis = Redis.GetRedis();
    var ttl = TimeSpan.FromHours(8);
    return new RedisArticleRepository(redis, ttl);
});
builder.Services.AddScoped<IArticleCache, ArticleCache>();
builder.Services.AddSingleton<ISearchLogger, SearchLogger>();
builder.Services.AddSingleton<CacheQueue>();

builder.Services.AddMassTransit(conf =>
{
    conf.SetKebabCaseEndpointNameFormatter();
    conf.AddConsumers(Assembly.GetExecutingAssembly());
    conf.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(new Uri(builder.Configuration.GetValue<string>("RabbitMqSettings:Host")!), x =>
        {
            x.Username(builder.Configuration.GetValue<string>("RabbitMqSettings:Username")!);
            x.Password(builder.Configuration.GetValue<string>("RabbitMqSettings:Password")!);
        });
        var queueName = $"currency-rates-updated-{Environment.MachineName}";
        configurator.ReceiveEndpoint(queueName, e =>
        {
            e.ConfigureConsumer<CurrencyRatesChangedConsumer>(context);
            e.ConfigureConsumer<MarkupGroupChangedConsumer>(context);
        });
    });
});

builder.Services.AddHostedService<SearchLogBackgroundWorker>();
builder.Services.AddHostedService(p => p.GetRequiredService<CacheQueue>());


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
Redis.Configure(builder.Configuration["ConnectionStrings:RedisConnection"]!);
MapsterConfig.Configure();
SortByConfig.Configure();
DbTransactionConfig.Configure();
await SetupPriceGenerator.SetupPricesAsync(app.Services);

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
