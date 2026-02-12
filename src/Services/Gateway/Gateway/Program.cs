using System.Security.Claims;
using System.Text;
using Api.Common.Logging;
using Gateway.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using Security.Utils;
using Serilog;
using Serilog.Sinks.Loki;
using Serilog.Sinks.Loki.Labels;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);
var certsPath = Environment.GetEnvironmentVariable("CERTS_PATH");
if (!string.IsNullOrWhiteSpace(certsPath))
    Certs.RegisterCerts(certsPath);

builder.Configuration.AddJsonFromDirectory("ReverseProxy");

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
                new LokiLabel("service", "gateway"),
                new LokiLabel(
                    "env",
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown"
                )
            ])
        })
    )
    .CreateLogger();

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddProcessInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
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
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtBearer:IssuerSigningKey"]!)),
    };
});

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build());

var secret = Environment.GetEnvironmentVariable("GATEWAY_SUPER_KEY");

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.CopyRequestHeaders = true;

        builderContext.AddRequestTransform(transformContext =>
        {
            var headers = transformContext.ProxyRequest.Headers;
            var user = transformContext.HttpContext.User;

            headers.Remove("X-Gateway-Token");
            headers.Remove("X-User-Id");
            headers.Remove("X-Roles");
            headers.Remove("X-Permissions");

            headers.Add("X-Gateway-Token", secret);

            if (user.Identity?.IsAuthenticated != true)
                return ValueTask.CompletedTask;

            var userId = user.FindFirst("sub")?.Value ??
                         user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
                headers.Add("X-User-Id", userId);

            var roles = user.FindAll(ClaimTypes.Role)
                .Select(r => r.Value)
                .Distinct();

            headers.Add("X-Roles", string.Join(",", roles));

            var permissions = user.FindAll("permission")
                .Select(p => p.Value)
                .Distinct();

            headers.Add("X-Permissions", string.Join(",", permissions));

            return ValueTask.CompletedTask;
        });
    });

builder.Host.UseSerilog();

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
app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapMethods("{**any}", ["OPTIONS"], () => Results.Ok())
    .AllowAnonymous();

app.MapReverseProxy();

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.Run();