using Core.Interfaces;
using Core.Interfaces.Validators;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Security;

public static class ServiceProvider
{
    public static IServiceCollection AddSecurityLayer(this IServiceCollection collection, PasswordRules? passwordRules = null)
    {
        collection.AddSingleton<IJwtGenerator, JwtGenerator>();
        collection.AddSingleton<IPasswordManager, PasswordManager>();
        collection.AddSingleton(passwordRules ?? new PasswordRules());
        collection.AddSingleton<ITokenHasher, TokenHasher>();
        return collection;
    }
}