using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NamedObject;
using Notification.Channels.Email;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Recipients;
using Notification.Dequeuers;
using Notification.Interfaces;
using Notification.Options;
using Notification.Renderers;
using RazorLight;

namespace Notification.Extensions;

public static class EmailNotificationServiceCollectionExtensions
{
	public static IServiceCollection UseEmailNotifications(
		this IServiceCollection services,
		string? templatesRoot = null)
	{
		templatesRoot ??= Path.Combine(AppContext.BaseDirectory, "Templates", "Emails");

		services.AddNamedObjectRegistry();
		services.TryAddEnumerable(ServiceDescriptor.Scoped<INotificationChannel, EmailChannel>());
		services.TryAddSingleton<IRazorLightEngine>(_ => new RazorLightEngineBuilder()
			.UseFileSystemProject(templatesRoot)
			.UseMemoryCachingProvider()
			.Build());

		services.AddOptions<EmailChannelOptions>()
			.BindConfiguration(EmailChannelOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.TryAddSingleton<IEmailSender, EmailSender>();
		services.AddSingleton<Dequeuer<EmailRecipient>>(sp =>
			new Dequeuer<EmailRecipient>(
				systemName: EmailRecipient.ChannelName,
				logger: sp.GetRequiredService<ILogger<Dequeuer<EmailRecipient>>>(),
				options: sp.GetRequiredService<IOptions<EmailChannelOptions>>().Value,
				scopeFactory: sp.GetRequiredService<IServiceScopeFactory>()));

		services.AddHostedService(sp => sp.GetRequiredService<Dequeuer<EmailRecipient>>());

		return services;
	}
}
