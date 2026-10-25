using Application.Common.Models;
using Enums;
using FluentAssertions;
using Main.Application.Handlers.Users.GetUsers;
using Main.Entities.User;
using Main.Enums;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Users;

public sealed class GetUsersTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task Filters_ApplyTogetherAndEscapeLiteralWildcards()
	{
		var expected = new UserBuilder(Faker).WithUserName("filter-target-user")
			.WithUserInfo("Николай", "Петров", "Contains 100%_proof literally")
			.WithEmail("filter.person@contacts.example").WithRole(Role.Admin.ToString()).Build();
		expected.AddUserPhone("+7 (999) 222-33-44", PhoneType.Unknown, false, false);
		Context.Add(expected);
		await Context.SaveChangesAsync(CancellationToken);
		await CreateUser("filter-other-user", "Николай", "Петров");

		var unfiltered = new GetUsersQuery(new Pagination(0, 25), null, null, null, null, null, null, null, null);
		foreach (var (field, filter) in new (string, GetUsersQuery)[]
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
			(await Mediator.Send(filter, CancellationToken)).Users.Select(user => user.Id)
				.Should().Contain(expected.Id, $"the {field} filter must match this user");

		var result = await Mediator.Send(new GetUsersQuery(
			new Pagination(0, 10), "ник", "пет", "FILTER.PERSON@CONTACTS.EXAMPLE", "+7 (999) 222",
			"FILTER-TARGET", expected.Id, "100%_proof", ["admin"]), CancellationToken);
		result.Users.Select(user => user.Id).Should().Equal(expected.Id);

		var wildcard = await Mediator.Send(new GetUsersQuery(
			new Pagination(0, 10), "%", null, null, null, null, null, null, null), CancellationToken);
		wildcard.Users.Should().BeEmpty();
	}

	[Fact]
	public async Task Filters_PaginateWithoutLosingUsersWithoutProfiles()
	{
		var users = new List<User>();
		for (var index = 0; index < 3; index++)
			users.Add(await new UserBuilder(Faker).WithUserName($"no-profile-filter-{index}").BuildAndAddToDb(Context));

		var first = await Mediator.Send(new GetUsersQuery(
			new Pagination(0, 2), null, null, null, null, "no-profile-filter", null, null, null), CancellationToken);
		var second = await Mediator.Send(new GetUsersQuery(
			new Pagination(1, 2), null, null, null, null, "no-profile-filter", null, null, null), CancellationToken);

		first.Users.Concat(second.Users).Select(user => user.Id)
			.Should().Equal(users.Select(user => user.Id).Order());
	}

	private Task<User> CreateUser(string login, string name, string surname, string? description = null) =>
		new UserBuilder(Faker).WithUserName(login).WithUserInfo(name, surname, description)
			.BuildAndAddToDb(Context);
}
