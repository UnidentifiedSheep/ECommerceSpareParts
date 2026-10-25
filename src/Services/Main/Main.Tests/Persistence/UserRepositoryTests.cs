using FluentAssertions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.Persistence;

public sealed class UserRepositoryTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task Search_AllowsCompositionWithoutTrackingUsers()
	{
		var expected = await new UserBuilder(Faker).WithUserName("repository-search-target")
			.BuildAndAddToDb(Context);
		await new UserBuilder(Faker).WithUserName("repository-search-other").BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();
		var repository = Scope.ServiceProvider.GetRequiredService<IUserRepository>();

		var users = await repository.Search(Context.Users, "repository")
			.Where(user => user.Id == expected.Id)
			.Take(1)
			.ToListAsync(CancellationToken);

		users.Should().ContainSingle().Which.Id.Should().Be(expected.Id);
		Context.ChangeTracker.Entries<User>().Should().BeEmpty();
	}
	[Fact]
	public async Task SearchWithRank_RespectsSourceFiltersAndReturnsUntrackedUsersInRelevanceOrder()
	{
		var identityMatch = await new UserBuilder(Faker).WithUserName("rank-identity-match")
			.WithUserInfo("Космонавтов", "Петров", null).BuildAndAddToDb(Context);
		var descriptionMatch = await new UserBuilder(Faker).WithUserName("rank-description-match")
			.WithUserInfo("Николай", "Иванов", "Космонавтов").BuildAndAddToDb(Context);
		var excluded = await new UserBuilder(Faker).WithUserName("rank-excluded-match")
			.WithUserInfo("Космонавтов", "Сидоров", null).BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();
		var repository = Scope.ServiceProvider.GetRequiredService<IUserRepository>();

		var matches = await repository.SearchWithRank(
			Context.Users.Where(user => user.Id != excluded.Id), "космонавтов")
			.ToListAsync(CancellationToken);

		matches.Select(match => match.User.Id).Should().Equal(identityMatch.Id, descriptionMatch.Id);
		matches.Should().OnlyContain(match => match.Rank > 0);
		matches[0].Rank.Should().BeGreaterThan(matches[1].Rank);
		Context.ChangeTracker.Entries<User>().Should().BeEmpty();
	}

}
