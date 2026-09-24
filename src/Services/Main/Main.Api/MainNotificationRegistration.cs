using Main.Application.Notifications;
using Notification.Extensions;

namespace Main.Api;

public static class MainNotificationRegistration
{
	public static IServiceCollection AddMainNotifications(this IServiceCollection services)
	{
		services.AddNotificationServices();
		services.UseEmailNotifications();

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
