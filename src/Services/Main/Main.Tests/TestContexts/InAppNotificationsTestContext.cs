using Main.Entities.User;
using Main.Persistence.Context;
using Notification.Core.Entities;
using Tests.Abstractions;
using Tests.DataBuilders;
using Tests.Interfaces;

namespace Tests.TestContexts;

public class InAppNotificationsTestContext(DContext context, UsersTestContext usersTestContext)
	: TestContextBase<DContext>(context), IDependentTestContext
{
	public static Type[] DependsOn { get; } = [typeof(UsersTestContext)];

	public IReadOnlyList<User> Users { get; private set; } = null!;
	public IReadOnlyList<InAppNotification> Notifications { get; private set; } = null!;

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		Users = usersTestContext.Users.ToArray();
		var notifications = Users.Take(2)
			.SelectMany(user =>
				new InAppNotificationBuilder(Faker)
					.WithUserId(user.Id)
					.WithSeen()
					.BuildMany(2)
				.Concat(new InAppNotificationBuilder(Faker)
					.WithUserId(user.Id)
					.BuildMany(3)))
			.ToArray();

		await DbContext.AddRangeAsync(notifications, cancellationToken);
		await DbContext.SaveChangesAsync(cancellationToken);

		Notifications = notifications;
	}
}
