using Main.Entities.User;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders;
using Tests.Interfaces;

namespace Tests.TestContexts;

public class NotificationPreferencesTestContext(
	DContext context,
	UsersTestContext usersTestContext
	) : TestContextBase<DContext>(context), IDependentTestContext
{
	public static Type[] DependsOn { get; } = [typeof(UsersTestContext)];

	public IReadOnlyList<User> Users { get; private set; } = null!;
	public IReadOnlyList<UserNotificationPreference> Preferences { get; private set; } = null!;

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		Users = usersTestContext.Users.ToArray();
		var preferences = new[]
		{
			new UserNotificationPreferenceBuilder(Faker)
				.WithUserId(Users[0].Id)
				.WithEnabled(false)
				.Build(),
			new UserNotificationPreferenceBuilder(Faker)
				.WithUserId(Users[1].Id)
				.Build()
		};

		await DbContext.AddRangeAsync(preferences, cancellationToken);
		await DbContext.SaveChangesAsync(cancellationToken);
		Preferences = preferences;
	}
}
