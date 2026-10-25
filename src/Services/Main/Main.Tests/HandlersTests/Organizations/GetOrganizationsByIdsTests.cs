using FluentAssertions;
using Main.Application.Handlers.Organizations;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Organizations;

public class GetOrganizationsByIdsTests : IntegrationTest
{
	public GetOrganizationsByIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<UserRelationsTestContext>();
	}

	private UserRelationsTestContext TestContext => GetContext<UserRelationsTestContext>();

	[Fact]
	public async Task Batch_ReturnsOnlyRequestedOrganizationsWithoutDuplicates()
	{
		var firstId = TestContext.FirstUserOrganization.Id;
		var sharedId = TestContext.SharedOrganization.Id;

		var result = await Mediator.Send(
			new GetOrganizationsByIdsQuery([firstId, sharedId, firstId, Guid.NewGuid()]), CancellationToken);

		result.Organizations.Keys.Should().BeEquivalentTo([firstId, sharedId]);
		result.Organizations[firstId].Id.Should().Be(firstId);
		result.Organizations[sharedId].Id.Should().Be(sharedId);
	}

	[Fact]
	public async Task KnownHiddenOrganization_IsReturnedWhileSystemOrganizationIsExcluded()
	{
		var hiddenId = TestContext.HiddenOrganization.Id;
		var systemId = GetContext<UserContextTestContext>().SystemUser.Id;

		var result = await Mediator.Send(new GetOrganizationsByIdsQuery([hiddenId, systemId]), CancellationToken);

		result.Organizations.Keys.Should().Equal(hiddenId);
	}

	[Fact]
	public async Task IndividualOrganization_IsReturnedById()
	{
		var id = TestContext.IndividualOnlyUser.Id;

		var result = await Mediator.Send(new GetOrganizationsByIdsQuery(id), CancellationToken);

		result.Organizations.Keys.Should().Equal(id);
	}

	[Fact]
	public async Task MissingOrganization_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetOrganizationsByIdsQuery(Guid.NewGuid()), CancellationToken);

		result.Organizations.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetOrganizationsByIdsQuery(Array.Empty<Guid>()), CancellationToken);

		result.Organizations.Should().BeEmpty();
	}
}
