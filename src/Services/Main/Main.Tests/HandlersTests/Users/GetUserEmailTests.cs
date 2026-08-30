using FluentAssertions;
using Main.Application.Handlers.Users.GetUserEmail;
using Main.Entities.Exceptions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Users;

public class GetUserEmailTests : IntegrationTest
{
	public GetUserEmailTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<UserEmailTestContext>();
	}

	private UserEmailTestContext TestContext => GetContext<UserEmailTestContext>();

	[Fact]
	public async Task ExistingEmail_ReturnsProjectedEmail()
	{
		var result = await Mediator.Send(
			new GetUserEmailQuery(TestContext.User.Id, TestContext.Email.GetId()));

		result.Email.Email.Should().Be(TestContext.Email.GetId());
		result.Email.EmailType.Should().Be(TestContext.Email.EmailType);
		result.Email.IsPrimary.Should().BeTrue();
		result.Email.Confirmed.Should().BeTrue();
	}

	[Fact]
	public async Task EmailBelongsToAnotherUser_ThrowsNotFoundException()
	{
		var act = () => Mediator.Send(new GetUserEmailQuery(Guid.NewGuid(), TestContext.Email.GetId()));

		await act.Should().ThrowAsync<UserEmailNotFoundException>();
	}
}
