using Application.Common.Models;
using Enums;
using FluentAssertions;
using Main.Entities.Enums;
using Main.Entities.User;
using Main.Modules.Identity.Application.Queries.Users.GetUsers;
using Tests.DataBuilders.User;

namespace Tests.HandlersTests.Users;

public sealed class GetUsersTests(ReadScenarioFixture<GetUsersTests.UsersScenario> fixture)
	: ReadScenarioTest<GetUsersTests.UsersScenario>(fixture)
{
	[Fact]
	public async Task Filters_ApplyTogetherAndEscapeLiteralWildcards()
	{
		var expected = Scenario.FilterTarget;
		var unfiltered = new GetUsersQuery(
			new Pagination(0, 25),
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null);
		foreach (var (field, filter) in new[]
				{
					("name", unfiltered with { Name = "ник" }),
					("surname", unfiltered with { Surname = "пет" }),
					("email", unfiltered with { Email = "FILTER.PERSON@CONTACTS.EXAMPLE" }),
					("phone", unfiltered with { Phone = "+7 (999) 222" }),
					("login", unfiltered with { UserName = "FILTER-TARGET" }),
					("id", unfiltered with { Id = expected.Id }),
					("description", unfiltered with { Description = "100%_proof" }),
					("roles", unfiltered with { Roles = ["admin"] })
				})
			(await Mediator.Send(filter, CancellationToken)).Users
				.Select(user => user.Id)
				.Should()
				.Contain(expected.Id, $"the {field} filter must match this user");

		var result = await Mediator.Send(
			new GetUsersQuery(
				new Pagination(0, 10),
				"ник",
				"пет",
				"FILTER.PERSON@CONTACTS.EXAMPLE",
				"+7 (999) 222",
				"FILTER-TARGET",
				expected.Id,
				"100%_proof",
				["admin"]),
			CancellationToken);
		result.Users.Select(user => user.Id).Should().Equal(expected.Id);

		var wildcard = await Mediator.Send(
			new GetUsersQuery(
				new Pagination(0, 10),
				"%",
				null,
				null,
				null,
				null,
				null,
				null,
				null),
			CancellationToken);
		wildcard.Users.Should().BeEmpty();
	}

	[Fact]
	public async Task Filters_PaginateWithoutLosingUsersWithoutProfiles()
	{
		var first = await Mediator.Send(
			new GetUsersQuery(
				new Pagination(0, 2),
				null,
				null,
				null,
				null,
				"no-profile-filter",
				null,
				null,
				null),
			CancellationToken);
		var second = await Mediator.Send(
			new GetUsersQuery(
				new Pagination(1, 2),
				null,
				null,
				null,
				null,
				"no-profile-filter",
				null,
				null,
				null),
			CancellationToken);

		first.Users
			.Concat(second.Users)
			.Select(user => user.Id)
			.Should()
			.Equal(Scenario.UsersWithoutProfiles.Select(user => user.Id).Order());
	}

	public sealed class UsersScenario : ReadScenario
	{
		public User FilterTarget { get; private set; } = null!;

		public IReadOnlyList<User> UsersWithoutProfiles { get; private set; } = null!;

		protected override IEnumerable<object> Seed()
		{
			FilterTarget = new UserBuilder(Faker).WithUserName("filter-target-user")
				.WithUserInfo(
					"Николай",
					"Петров",
					"Contains 100%_proof literally")
				.WithEmail("filter.person@contacts.example")
				.WithRole(Role.Admin.ToString())
				.Build();
			FilterTarget.AddUserPhone(
				"+7 (999) 222-33-44",
				PhoneType.Unknown,
				false,
				false);
			var other = new UserBuilder(Faker).WithUserName("filter-other-user")
				.WithUserInfo("Николай", "Петров", null)
				.Build();
			UsersWithoutProfiles = Enumerable.Range(0, 3)
				.Select(index => new UserBuilder(Faker).WithUserName($"no-profile-filter-{index}").Build())
				.ToArray();
			return [FilterTarget, other, ..UsersWithoutProfiles];
		}
	}
}
