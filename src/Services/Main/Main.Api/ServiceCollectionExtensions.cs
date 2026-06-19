using Abstractions.Models;
using Abstractions.Models.Options;
using Main.Application.Models;

namespace Main.Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailOptions(this IServiceCollection sc)
    {
        sc.AddOptions<UserEmailOptions>()
            .BindConfiguration(UserEmailOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    
        return sc;
    }

    public static IServiceCollection AddPhoneOptions(this IServiceCollection sc)
    {
        sc.AddOptions<UserPhoneOptions>()
            .BindConfiguration(UserPhoneOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return sc;
    }

    public static IServiceCollection AddJwtOptions(this IServiceCollection sc)
    {
        sc.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return sc;
    }
}