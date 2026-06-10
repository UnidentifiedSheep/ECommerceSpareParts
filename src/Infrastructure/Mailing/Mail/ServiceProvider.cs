using Abstractions.Interfaces.Mail;
using EmailTemplates.Renderer;
using Mailing.Core;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;
using RazorLight.Extensions;

namespace Mail;

public static class ServiceProvider
{
    public static IServiceCollection AddMailLayer(
        this IServiceCollection collection,
        string? templatesRoot = null)
    {
        templatesRoot ??= Path.Combine(
            AppContext.BaseDirectory,
            "Templates",
            "Emails");
        
        collection.AddOptions<MailOptions>()
            .BindConfiguration(MailOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        collection.AddSingleton<IEmailSender, EmailSender>();
        
        collection.AddRazorLight(() => new RazorLightEngineBuilder()
            .UseFileSystemProject(templatesRoot)
            .UseMemoryCachingProvider()
            .Build());
        collection.AddSingleton<IEmailMessageRenderer, EmailMessageRenderer>();
        
        return collection;
    }
}