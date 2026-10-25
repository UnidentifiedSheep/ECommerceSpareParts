using Main.Entities.User;
using Main.Enums;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.User;
using Tests.Extensions;

namespace Tests.TestContexts;

public class UserContactsTestContext(DContext context) : TestContextBase<DContext>(context)
{
	public User FirstUser { get; private set; } = null!;

	public User SecondUser { get; private set; } = null!;

	public User OtherUser { get; private set; } = null!;

	public User UserWithoutContacts { get; private set; } = null!;

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		FirstUser = await new UserBuilder(Faker)
			.WithUserName("first-contact-user")
			.WithEmail("personal@example.com", EmailType.Personal, isConfirmed: false)
			.WithEmail("work@example.com", EmailType.Work, isPrimary: true)
			.WithPhone("+1 (555) 123-4567", isConfirmed: false)
			.WithPhone("+1 (555) 765-4321", PhoneType.Work, isPrimary: true)
			.BuildAndAddToDb(DbContext);

		SecondUser = await new UserBuilder(Faker)
			.WithUserName("second-contact-user")
			.WithEmail("second@example.com", EmailType.Personal)
			.WithPhone("+1 (555) 222-3333")
			.BuildAndAddToDb(DbContext);

		OtherUser = await new UserBuilder(Faker)
			.WithUserName("other-contact-user")
			.WithEmail("excluded@example.com", EmailType.Work, isPrimary: true)
			.WithPhone("+1 (999) 999-9999", PhoneType.Work, isPrimary: true)
			.BuildAndAddToDb(DbContext);

		UserWithoutContacts = await new UserBuilder(Faker)
			.WithUserName("user-without-contacts")
			.BuildAndAddToDb(DbContext);
	}
}
