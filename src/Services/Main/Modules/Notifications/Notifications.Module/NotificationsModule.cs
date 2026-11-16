using System.Reflection;
using Application.Common.Host;
using Main.Modules.Notifications.Application;
using Main.Modules.Notifications.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Modules.Notifications;

public sealed class NotificationsModule : ModuleBase
{
	public override IReadOnlyCollection<Assembly> ConsumerAssemblies
		=> [NotificationsApplicationAssembly.Value];

	public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddNotificationsApplication();
		services.AddNotificationsPersistence();
	}
}
