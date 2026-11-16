using Main.Modules.Notifications.Application.Delivery.Channels.Email;
using Main.Modules.Notifications.Application.Delivery.Dequeuers;
using Main.Modules.Notifications.Application.Delivery.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NamedObject;
using Notification.Core.Interfaces.Notification;
using Notification.Core.Recipients;
using RazorLight;

namespace Main.Modules.Notifications.Application.Delivery.Extensions;

public static class EmailNotificationServiceCollectionExtensions
{
	public static IServiceCollection UseEmailNotifications(
		this IServiceCollection services,
		string? templatesRoot = null)
	{
		templatesRoot ??= Path.Combine(
			AppContext.BaseDirectory,
			"Templates",
			"Emails");

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

		return services;
	}

	public static IServiceCollection AddEmailNotificationHostedService(this IServiceCollection services)
	{
		services.AddSingleton<Dequeuer<EmailRecipient>>(sp =>
			new Dequeuer<EmailRecipient>(
				EmailRecipient.ChannelName,
				sp.GetRequiredService<ILogger<Dequeuer<EmailRecipient>>>(),
				sp.GetRequiredService<IOptions<EmailChannelOptions>>().Value,
				sp.GetRequiredService<IServiceScopeFactory>()));

		services.AddHostedService(sp => sp.GetRequiredService<Dequeuer<EmailRecipient>>());
		return services;
	}
}
