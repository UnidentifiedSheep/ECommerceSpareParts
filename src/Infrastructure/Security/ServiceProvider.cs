using System.Text;
using System.Text.Json;
using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Abstractions.Interfaces.Validators;
using Abstractions.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Security.Services;

namespace Security;

public static class ServiceProvider
{
    public static IServiceCollection AddFullSecurityLayer(
        this IServiceCollection collection,
        PasswordRules? passwordRules = null)
    {
        collection.AddSingleton<IPasswordManager, PasswordManager>();
        collection.AddSingleton(passwordRules ?? new PasswordRules());
        collection.AddSingleton<ITokenHasher, TokenHasher>();

        collection.AddMinimalSecurityLayer();
        return collection;
    }

    public static IServiceCollection AddJsonSigner(
        this IServiceCollection collection,
        string secret,
        JsonSerializerOptions? options = null)
    {
        collection.AddSingleton<IJsonSigner, JsonSigner>(_ => new JsonSigner(secret, options));
        return collection;
    }

    public static IServiceCollection AddMinimalSecurityLayer(this IServiceCollection collection)
    {
        collection.TryAddScoped<IUserContext, UserContext>();
        return collection;
    }

    public static IServiceCollection AddEComAuth(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        collection.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var iss = configuration["JwtBearer:ValidIssuer"];
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = iss,
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtBearer:IssuerSigningKey"]!))
            };
        });

        collection.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build());
        
        return collection;
    }
}