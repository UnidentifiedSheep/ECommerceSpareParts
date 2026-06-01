using Abstractions.Interfaces.Mail;
using Microsoft.Extensions.DependencyInjection;

namespace Mail;

public static class ServiceProvider
{
    public static IServiceCollection AddMailLayer(this IServiceCollection collection)
    {
        collection.AddOptions<MailOptions>()
            .BindConfiguration(MailOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        collection.AddSingleton<IEmailSender, EmailSender>();
        
        return collection;
    }
}