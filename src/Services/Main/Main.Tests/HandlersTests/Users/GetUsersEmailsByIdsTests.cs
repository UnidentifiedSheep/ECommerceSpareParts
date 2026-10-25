using FluentAssertions;
using Main.Application.Handlers.Users;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Users;

public class GetUsersEmailsByIdsTests : IntegrationTest
{
	public GetUsersEmailsByIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<UserContactsTestContext>();
	}

	private UserContactsTestContext TestContext => GetContext<UserContactsTestContext>();

	[Fact]
	public async Task Batch_ReturnsRequestedUsersEmailsWithPrimaryFirst()
	{
		var first = TestContext.FirstUser;
		var second = TestContext.SecondUser;
		var users = new[] { first, second };

		var result = await Mediator.Send(
			new GetUsersEmailsByIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]), CancellationToken);

		result.Emails.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
		result.Emails[first.Id][0].IsPrimary.Should().BeTrue();
		foreach (var user in users)
		{
			var actual = result.Emails[user.Id];
			actual.Should().HaveCount(user.Emails.Count);
			actual.Select(x => x.Email).Should().BeEquivalentTo(user.Emails.Select(x => x.GetId()));
			foreach (var email in user.Emails)
			{
				var projected = actual.Single(x => x.Email == email.GetId());
				projected.EmailType.Should().Be(email.EmailType);
				projected.IsPrimary.Should().Be(email.IsPrimary);
				projected.Confirmed.Should().Be(email.Confirmed);
				projected.CreatedAt.Should().BeCloseTo(email.CreatedAt, TimeSpan.FromMicroseconds(1));
				projected.UpdatedAt.Should().BeCloseTo(email.UpdatedAt, TimeSpan.FromMicroseconds(1));
				if (email.ConfirmedAt is { } confirmedAt)
					projected.ConfirmedAt.Should().BeCloseTo(confirmedAt, TimeSpan.FromMicroseconds(1));
				else
					projected.ConfirmedAt.Should().BeNull();
			}
		}
	}

	[Fact]
	public async Task SingleUser_ReturnsTheirEmails()
	{
		var user = TestContext.FirstUser;

		var result = await Mediator.Send(new GetUsersEmailsByIdsQuery(user.Id), CancellationToken);

		result.Emails.Keys.Should().Equal(user.Id);
		result.Emails[user.Id].Should().HaveCount(user.Emails.Count);
		result.Emails[user.Id][0].IsPrimary.Should().BeTrue();
	}

	[Fact]
	public async Task ExistingUserWithoutEmails_ReturnsEmptyDictionary()
	{
		var user = TestContext.UserWithoutContacts;

		var result = await Mediator.Send(new GetUsersEmailsByIdsQuery(user.Id), CancellationToken);

		result.Emails.Should().BeEmpty();
	}

	[Fact]
	public async Task MissingSingleUser_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetUsersEmailsByIdsQuery(Guid.NewGuid()), CancellationToken);

		result.Emails.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetUsersEmailsByIdsQuery(Array.Empty<Guid>()), CancellationToken);

		result.Emails.Should().BeEmpty();
	}
}
