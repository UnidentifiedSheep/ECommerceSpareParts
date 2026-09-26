using Main.Application.Notifications;
using Main.Application.Notifications.DeliveryObservers;
using Notification.Core.Interfaces;
using Notification.Core.Recipients;
using Notification.Extensions;

namespace Main.Api;

public static class MainNotificationRegistration
{
	public static IServiceCollection AddMainNotifications(this IServiceCollection services)
	{
		services.AddNotificationServices();
		services.UseEmailNotifications();
		services.AddNotifications();

		services.AddScoped<IChannelDeliveryObserver<InAppReceipt, InAppRecipient>, InAppDeliveryObserver>();

		return services;
	}

	public static IServiceCollection AddNotifications(this IServiceCollection services)
	{
		services.AddNotification<PasswordResetNotification, PasswordResetNotificationData>(
			"PasswordReset",
			model => new PasswordResetNotification(model));
		services.AddNotification<EmailVerificationNotification, EmailVerificationNotificationData>(
			"EmailVerification",
			model => new EmailVerificationNotification(model));
		services.AddNotification<UserLoggedInNotification, UserLoggedInNotificationData>(
			"UserLoggedIn",
			model => new UserLoggedInNotification(model));

		return services;
	}
}
