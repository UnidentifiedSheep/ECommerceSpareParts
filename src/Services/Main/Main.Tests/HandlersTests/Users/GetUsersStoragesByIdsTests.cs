using FluentAssertions;
using Main.Application.Handlers.Users;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Users;

public class GetUsersStoragesByIdsTests : IntegrationTest
{
	public GetUsersStoragesByIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<UserRelationsTestContext>();
	}

	private UserRelationsTestContext TestContext => GetContext<UserRelationsTestContext>();

	[Fact]
	public async Task Batch_ReturnsOnlyRequestedUsersStoragesIncludingSharedRelations()
	{
		var first = TestContext.FirstUser;
		var second = TestContext.SecondUser;

		var result = await Mediator.Send(
			new GetUsersStoragesByIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]), CancellationToken);

		result.Storages.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
		result.Storages[first.Id].Select(x => x.Code).Should().Equal([
			TestContext.FirstUserStorage.Code, TestContext.SharedStorage.Code]);
		result.Storages[second.Id].Should().ContainSingle().Which.Code.Should().Be(TestContext.SharedStorage.Code);
	}

	[Fact]
	public async Task SingleUser_ReturnsProjectedStorages()
	{
		var user = TestContext.FirstUser;

		var result = await Mediator.Send(new GetUsersStoragesByIdsQuery(user.Id), CancellationToken);

		result.Storages.Keys.Should().Equal(user.Id);
		result.Storages[user.Id].Should().HaveCount(2);
		var storage = result.Storages[user.Id].Single(x => x.Code == TestContext.SharedStorage.Code);
		storage.Code.Should().Be(TestContext.SharedStorage.Code);
		storage.Type.Should().Be(TestContext.SharedStorage.Type);
		storage.Description.Should().Be(TestContext.SharedStorage.Description);
		storage.Location.Should().Be(TestContext.SharedStorage.Location);
		var ownStorage = result.Storages[user.Id].Single(x => x.Code == TestContext.FirstUserStorage.Code);
		ownStorage.Description.Should().BeNull();
		ownStorage.Location.Should().BeNull();
	}

	[Fact]
	public async Task IndividualOnlyUser_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(
			new GetUsersStoragesByIdsQuery(TestContext.IndividualOnlyUser.Id), CancellationToken);

		result.Storages.Should().BeEmpty();
	}

	[Fact]
	public async Task MissingUser_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetUsersStoragesByIdsQuery(Guid.NewGuid()), CancellationToken);

		result.Storages.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetUsersStoragesByIdsQuery(Array.Empty<Guid>()), CancellationToken);

		result.Storages.Should().BeEmpty();
	}
}
