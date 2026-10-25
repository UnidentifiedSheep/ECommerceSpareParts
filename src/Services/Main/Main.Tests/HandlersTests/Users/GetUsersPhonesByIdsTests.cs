using FluentAssertions;
using Main.Application.Handlers.Users;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Users;

public class GetUsersPhonesByIdsTests : IntegrationTest
{
	public GetUsersPhonesByIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<UserContactsTestContext>();
	}

	private UserContactsTestContext TestContext => GetContext<UserContactsTestContext>();

	[Fact]
	public async Task Batch_ReturnsRequestedUsersPhonesWithPrimaryFirst()
	{
		var first = TestContext.FirstUser;
		var second = TestContext.SecondUser;
		var users = new[] { first, second };

		var result = await Mediator.Send(
			new GetUsersPhonesByIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]), CancellationToken);

		result.Phones.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
		result.Phones[first.Id][0].IsPrimary.Should().BeTrue();
		foreach (var user in users)
		{
			var actual = result.Phones[user.Id];
			actual.Should().HaveCount(user.Phones.Count);
			actual.Select(x => x.Number).Should().BeEquivalentTo(user.Phones.Select(x => x.PhoneNumber));
			foreach (var phone in user.Phones)
			{
				var projected = actual.Single(x => x.Number == phone.PhoneNumber);
				projected.Type.Should().Be(phone.PhoneType);
				projected.IsPrimary.Should().Be(phone.IsPrimary);
				projected.IsConfirmed.Should().Be(phone.Confirmed);
			}
		}
	}

	[Fact]
	public async Task SingleUser_ReturnsTheirPhones()
	{
		var user = TestContext.FirstUser;

		var result = await Mediator.Send(new GetUsersPhonesByIdsQuery(user.Id), CancellationToken);

		result.Phones.Keys.Should().Equal(user.Id);
		result.Phones[user.Id].Should().HaveCount(user.Phones.Count);
		result.Phones[user.Id][0].IsPrimary.Should().BeTrue();
	}

	[Fact]
	public async Task ExistingUserWithoutPhones_ReturnsEmptyDictionary()
	{
		var user = TestContext.UserWithoutContacts;

		var result = await Mediator.Send(new GetUsersPhonesByIdsQuery(user.Id), CancellationToken);

		result.Phones.Should().BeEmpty();
	}

	[Fact]
	public async Task MissingSingleUser_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetUsersPhonesByIdsQuery(Guid.NewGuid()), CancellationToken);

		result.Phones.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetUsersPhonesByIdsQuery([]), CancellationToken);

		result.Phones.Should().BeEmpty();
	}
}
