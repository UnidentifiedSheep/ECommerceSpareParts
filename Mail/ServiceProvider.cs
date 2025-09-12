using Mail.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mail;

public static class ServiceProvider
{
    public static IServiceCollection AddMailLayer(this IServiceCollection collection)
    {
        collection.AddTransient<IMail, Service.Mail>();
        return collection;
    }
}