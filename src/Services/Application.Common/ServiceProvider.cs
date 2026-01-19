using System.Reflection;
using Application.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common;

public static class ServiceProvider
{
    public static IServiceCollection RegisterDbValidations(this IServiceCollection services, Assembly? assembly)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        var validationTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.BaseType != null 
                        && t.BaseType.IsGenericType
                        && t.BaseType.GetGenericTypeDefinition() == typeof(AbstractDbValidation<>));

        foreach (var type in validationTypes)
        {
            var baseType = type.BaseType!;
            services.AddScoped(baseType, type);
        }

        return services;
    }
}