using Main.Entities.User;
using Main.Persistence.Context;
using Main.Enums;
using Tests.Abstractions;
using Tests.Interfaces;

namespace Tests.TestContexts;

public class UserEmailTestContext(
    DContext context,
    UsersTestContext usersTestContext)
    : TestContextBase<DContext>(context), IDependentTestContext
{
    public const string EmailAddress = "test-context@example.com";

    public User User { get; private set; } = null!;
    public UserEmail Email { get; private set; } = null!;

    public static Type[] DependsOn { get; } = [typeof(UsersTestContext)];

    public override async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        User = usersTestContext.Users.First();
        User.AddEmail(
            EmailAddress,
            EmailType.Work,
            isPrimary: true,
            isConfirmed: true);
        Email = User.Emails.Single(x => x.Email == EmailAddress);

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
