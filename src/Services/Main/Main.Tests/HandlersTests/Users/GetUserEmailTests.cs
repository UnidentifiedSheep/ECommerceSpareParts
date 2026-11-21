using FluentAssertions;
using Main.Entities.Exceptions;
using Main.Modules.Identity.Application.Queries.Users.GetUserEmail;
using Tests.TestContexts;

namespace Tests.HandlersTests.Users;

public class GetUserEmailTests(ReadScenarioFixture<GetUserEmailTests.GetUserEmailScenario> fixture)
	: ReadScenarioTest<GetUserEmailTests.GetUserEmailScenario>(fixture)
{
	private UserContactsTestContext TestContext => Scenario.Get<UserContactsTestContext>();

	[Fact]
	public async Task ExistingEmail_ReturnsProjectedEmail()
	{
		var user = TestContext.FirstUser;
		var email = user.Emails.Single(x => x.IsPrimary);

		var result = await Mediator.Send(
			new GetUserEmailQuery(user.Id, email.GetId()),
			CancellationToken);

		result.Email.Email.Should().Be(email.GetId());
		result.Email.EmailType.Should().Be(email.EmailType);
		result.Email.IsPrimary.Should().BeTrue();
		result.Email.Confirmed.Should().BeTrue();
	}

	[Fact]
	public async Task EmailBelongsToAnotherUser_ThrowsNotFoundException()
	{
		var email = TestContext.FirstUser.Emails.Single(x => x.IsPrimary);
		var act = () => Mediator.Send(new GetUserEmailQuery(TestContext.SecondUser.Id, email.GetId()));

		await act.Should().ThrowAsync<UserEmailNotFoundException>();
	}

	public sealed class GetUserEmailScenario : ReadScenario
	{
		public GetUserEmailScenario() => RegisterBasicContext<UserContactsTestContext>();

		protected override IEnumerable<object> Seed() => [];
	}
}
