using System.Text;
using Gateway.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFromDirectory("ReverseProxy");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AMW", policy => { policy.RequireRole("Admin", "Moderator", "Worker"); });
    options.AddPolicy("AM", policy => { policy.RequireRole("Admin", "Moderator"); });
    options.AddPolicy("MEMBER", policy => { policy.RequireRole("Member"); });
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
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = iss,
        ValidAudience = builder.Configuration["JwtBearer:ValidAudience"],
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtBearer:IssuerSigningKey"]!))
    };
});

var secret = builder.Configuration["Gateway:Secret"];

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.CopyRequestHeaders = true;
        builderContext.AddRequestTransform(transformContext =>
        {
            transformContext.ProxyRequest.Headers.Add("X-Gateway-Token", secret);
            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();