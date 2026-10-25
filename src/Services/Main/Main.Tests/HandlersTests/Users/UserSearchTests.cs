using Application.Common.Models;
using Enums;
using FluentAssertions;
using FluentValidation;
using Main.Application.Handlers.Users.SearchUsers;
using Main.Entities.User;
using Main.Enums;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Users;

public sealed class UserSearchTests : IntegrationTest
{
	public UserSearchTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<UsersTestContext>();
	}

	private const string RoleSearchTerm = "Фильтрролей";

	private UsersTestContext TestContext => GetContext<UsersTestContext>();

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
		var user = await new UserBuilder(Faker).WithUserName("contacts-only-user")
			.WithEmail("unique.person@contacts.example")
			.WithPhone("+7 (999) 123-45-67", PhoneType.Unknown, isConfirmed: false)
			.BuildAndAddToDb(Context);

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

	[Fact]
	public async Task Search_RolesMatchAnyRequestedRoleWithoutDuplicatesAndStillRequireTextMatch()
	{
		await PrepareRoleSearchProfiles();
		TestContext.ThirdUser.AddRole(TestContext.ReaderRole.Name);
		TestContext.ThirdUser.AddRole(TestContext.EditorRole.Name);
		await Context.SaveChangesAsync(CancellationToken);
		await new UserBuilder(Faker).WithRole(TestContext.ReaderRole.Name)
			.WithUserInfo("Другой", "Пользователь", null).BuildAndAddToDb(Context);

		var result = await Search(RoleSearchTerm,
			roles: [$" {TestContext.ReaderRole.Name.ToLowerInvariant()} ", TestContext.EditorRole.Name, TestContext.ReaderRole.Name]);

		result.Users.Select(user => user.Id).Should().BeEquivalentTo([
			TestContext.User.Id, TestContext.OtherUser.Id, TestContext.ThirdUser.Id]);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task Search_NullOrEmptyRolesDoNotFilterUsers(bool emptyRoles)
	{
		await PrepareRoleSearchProfiles();

		var result = await Search(RoleSearchTerm, roles: emptyRoles ? [] : null);

		result.Users.Select(user => user.Id).Should().BeEquivalentTo([
			TestContext.User.Id, TestContext.OtherUser.Id, TestContext.ThirdUser.Id]);
	}

	[Fact]
	public async Task Search_RoleFilterPreservesRankingAndAppliesBeforePagination()
	{
		await PrepareRoleSearchProfiles();
		var roles = new[] { TestContext.ReaderRole.Name, TestContext.EditorRole.Name };

		var first = await Search(RoleSearchTerm, new Pagination(0, 1), roles);
		var second = await Search(RoleSearchTerm, new Pagination(1, 1), roles);

		first.Users.Select(user => user.Id).Should().Equal(TestContext.User.Id);
		second.Users.Select(user => user.Id).Should().Equal(TestContext.OtherUser.Id);
		(await Search(RoleSearchTerm, new Pagination(2, 1), roles)).Users.Should().BeEmpty();
	}

	[Fact]
	public async Task Search_UnknownRoleReturnsNoUsers()
	{
		await PrepareRoleSearchProfiles();

		(await Search(RoleSearchTerm, roles: ["missing-role"])).Users.Should().BeEmpty();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task Search_SystemUsersAreExcludedEvenWhenTheirRolesAreRequested(bool includeReader)
	{
		await PrepareRoleSearchProfiles();
		TestContext.OtherUser.AddRole(nameof(Role.System));
		TestContext.OtherUser.AddRole(TestContext.ReaderRole.Name);
		await Context.SaveChangesAsync(CancellationToken);
		var roles = includeReader ? new[] { " system ", TestContext.ReaderRole.Name } : ["system"];

		var result = await Search(RoleSearchTerm, roles: roles);

		if (includeReader)
			result.Users.Select(user => user.Id).Should().Equal(TestContext.User.Id);
		else
			result.Users.Should().BeEmpty();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("\t\r\n")]
	public async Task Search_EmptyTermIsRejectedWithLocalizedMessage(string? term)
	{
		var exception = await Assert.ThrowsAsync<ValidationException>(() => Search(term!));

		exception.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(SearchUsersQuery.SearchTerm) && error.ErrorCode == "search.string.required");
	}

	[Fact]
	public async Task Search_TermOver200CharactersIsRejectedWithLocalizedMessage()
	{
		var exception = await Assert.ThrowsAsync<ValidationException>(() => Search(new string('a', 201)));

		exception.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(SearchUsersQuery.SearchTerm) && error.ErrorCode == "search.string.max.length");
	}

	[Fact]
	public async Task Search_TermOf200CharactersIsAccepted()
	{
		var term = new string('a', 200);
		var user = await CreateUser("search-length-boundary", "Граница", "Поиска", term);

		(await Search(term)).Users.Select(result => result.Id).Should().Equal(user.Id);
	}

	[Theory]
	[InlineData(-1, 25)]
	[InlineData(0, 0)]
	[InlineData(0, 101)]
	public async Task Search_InvalidPaginationIsRejected(int page, int size)
	{
		var exception = await Assert.ThrowsAsync<ValidationException>(() =>
			Search(RoleSearchTerm, new Pagination(page, size)));

		exception.Errors.Should().Contain(error => error.PropertyName.StartsWith("Pagination.", StringComparison.Ordinal));
	}

	private async Task PrepareRoleSearchProfiles()
	{
		TestContext.User.SetUserInfo(RoleSearchTerm, "Читатель", null);
		TestContext.OtherUser.SetUserInfo("Редактор", "Поиска", RoleSearchTerm);
		TestContext.ThirdUser.SetUserInfo($"{RoleSearchTerm} {RoleSearchTerm}", RoleSearchTerm, null);
		await Context.SaveChangesAsync(CancellationToken);
	}

	private Task<SearchUsersResult> Search(
		string term, Pagination? pagination = null, IReadOnlyCollection<string>? roles = null) =>
		Mediator.Send(new SearchUsersQuery(term, roles, pagination ?? new Pagination(0, 25)), CancellationToken);

	private Task<User> CreateUser(string login, string name, string surname, string? description = null) =>
		new UserBuilder(Faker).WithUserName(login).WithUserInfo(name, surname, description)
			.BuildAndAddToDb(Context);
}
