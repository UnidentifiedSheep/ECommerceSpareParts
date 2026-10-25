using Application.Common.Models;
using Enums;
using FluentAssertions;
using Main.Application.Handlers.Users.SearchUsers;
using Main.Entities.User;
using Main.Enums;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Users;

public sealed class UserSearchTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task Search_MatchesWordsInAnyOrderAndPrefixesDespiteLongDescription()
	{
		var user = await CreateUser("user-astronaut", "Александр", "Звездочетов", new string('я', 290));

		(await Search("звездоч але")).Users.Select(result => result.Id).Should().Equal(user.Id);
		(await Search("але звездоч")).Users.Select(result => result.Id).Should().Equal(user.Id);
		(await Search("звездоч несуществующий")).Users.Should().BeEmpty();
	}

	[Fact]
	public async Task Search_NormalizesYoWithoutStemmingNames()
	{
		var user = await CreateUser("user-birch", "Семен", "Березовский");

		(await Search("СЕМЁН БЕРЁЗ")).Users.Select(result => result.Id).Should().Equal(user.Id);
	}

	[Fact]
	public async Task Search_FindsLoginEmailAndFormattedPhoneWithoutProfile()
	{
		var user = new UserBuilder(Faker).WithUserName("contacts-only-user")
			.WithEmail("unique.person@contacts.example").Build();
		user.AddUserPhone("+7 (999) 123-45-67", PhoneType.Unknown, false, false);
		Context.Add(user);
		await Context.SaveChangesAsync(CancellationToken);

		(await Search("contacts-only")).Users.Select(result => result.Id).Should().Equal(user.Id);
		(await Search("unique.person@contacts.example")).Users.Select(result => result.Id).Should().Equal(user.Id);
		(await Search("+7 (999) 123-45-67")).Users.Select(result => result.Id).Should().Equal(user.Id);
	}

	[Fact]
	public async Task Search_RanksIdentityAboveDescription()
	{
		var descriptionMatch = await CreateUser("description-match", "Николай", "Петров", "Космонавтов");
		var identityMatch = await CreateUser("identity-match", "Космонавтов", "Андреев", new string('я', 290));

		(await Search("космонавтов")).Users.Select(result => result.Id)
			.Should().Equal(identityMatch.Id, descriptionMatch.Id);
	}

	[Fact]
	public async Task Search_ExcludesSystemUsersAndPaginatesEqualScoresById()
	{
		var users = new List<User>();
		for (var index = 0; index < 3; index++)
			users.Add(await CreateUser($"pagination-user-{index}", "Пагинация", "Одинаковая"));
		await new UserBuilder(Faker).WithUserName("pagination-system-user")
			.WithUserInfo("Пагинация", "Одинаковая", null).WithRole(Role.System.ToString())
			.BuildAndAddToDb(Context);

		var first = await Search("пагинация", new Pagination(0, 2));
		var second = await Search("пагинация", new Pagination(1, 2));

		first.Users.Concat(second.Users).Select(user => user.Id)
			.Should().Equal(users.Select(user => user.Id).Order());
	}

	[Fact]
	public async Task Search_PunctuationCannotBecomeTsQuerySyntax()
	{
		foreach (var term in new[] { "' | & ! :* ()", @"\path\with\backslashes", "https://invalid.example/a'b" })
			(await Search(term)).Users.Should().BeEmpty();
	}

	private Task<SearchUsersResult> Search(string term, Pagination? pagination = null) =>
		Mediator.Send(new SearchUsersQuery(term, pagination ?? new Pagination(0, 25)), CancellationToken);

	private Task<User> CreateUser(string login, string name, string surname, string? description = null) =>
		new UserBuilder(Faker).WithUserName(login).WithUserInfo(name, surname, description)
			.BuildAndAddToDb(Context);
}
