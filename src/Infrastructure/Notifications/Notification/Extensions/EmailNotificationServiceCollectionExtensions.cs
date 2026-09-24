using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NamedObject;
using Notification.Channels.Email;
using Notification.Core.Interfaces.Notification;
using Notification.Interfaces;
using Notification.Options;
using Notification.Renderers;

namespace Notification.Extensions;

public static class EmailNotificationServiceCollectionExtensions
{
	public static IServiceCollection UseEmailNotifications(this IServiceCollection services)
	{
		services.AddNamedObjectRegistry();
		services.TryAddEnumerable(ServiceDescriptor.Scoped<INotificationChannel, EmailChannel>());

		services.AddOptions<EmailChannelOptions>()
			.BindConfiguration(EmailChannelOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();
		services.TryAddSingleton<IEmailSender, EmailSender>();

		return services;
	}
}
