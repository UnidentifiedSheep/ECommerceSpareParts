using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Notifications.Persistence;

public sealed class NotificationsDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
		=> modelBuilder
			.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContextModule).Assembly);
}
