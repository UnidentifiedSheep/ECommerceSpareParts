using System.Security.Claims;
using System.Text;
using Api.Common.Extensions;
using Gateway.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);


var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
var lokiUrl = Environment.GetEnvironmentVariable("LOKI_URL");

builder.Configuration
    .AddConfigsFromJsons(env)
    .AddConfigsFromJsons(env, "/app/configs");

builder.Host.AddLokiLogger(builder.Configuration, "gateway", env, lokiUrl);

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
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtBearer:IssuerSigningKey"]!))
    };
});

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build());

var secret = builder.Configuration["HeaderSecret:Key"];

if (secret == null)
    throw new ArgumentNullException(nameof(secret), "HeaderSecret:Key cannot be null.");

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

app.MapReverseProxy();

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.Run();