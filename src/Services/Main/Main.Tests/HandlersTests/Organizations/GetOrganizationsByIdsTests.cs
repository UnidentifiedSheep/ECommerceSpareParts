using FluentAssertions;
using Main.Modules.Organization.Application.Queries.Organizations;
using Tests.TestContexts;

namespace Tests.HandlersTests.Organizations;

public class GetOrganizationsByIdsTests(ReadScenarioFixture<GetOrganizationsByIdsTests.GetOrganizationsByIdsScenario> fixture)
	: ReadScenarioTest<GetOrganizationsByIdsTests.GetOrganizationsByIdsScenario>(fixture)
{
	private UserRelationsTestContext TestContext => Scenario.Get<UserRelationsTestContext>();

	[Fact]
	public async Task Batch_ReturnsOnlyRequestedOrganizationsWithoutDuplicates()
	{
		var firstId = TestContext.FirstUserOrganization.Id;
		var sharedId = TestContext.SharedOrganization.Id;

		var result = await Mediator.Send(
			new GetOrganizationsByIdsQuery([firstId, sharedId, firstId, Guid.NewGuid()]),
			CancellationToken);

		result.Organizations.Keys.Should().BeEquivalentTo([firstId, sharedId]);
		result.Organizations[firstId].Id.Should().Be(firstId);
		result.Organizations[sharedId].Id.Should().Be(sharedId);
	}

	[Fact]
	public async Task HiddenAndSystemOrganizations_AreBothReturned()
	{
		var hiddenId = TestContext.HiddenOrganization.Id;
		var systemId = Scenario.Get<UserContextTestContext>().SystemUser.Id;

		var result = await Mediator.Send(
			new GetOrganizationsByIdsQuery([hiddenId, systemId]),
			CancellationToken);

		result.Organizations.Keys.Should().BeEquivalentTo([hiddenId, systemId]);
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
		var result = await Mediator.Send(
			new GetOrganizationsByIdsQuery(Array.Empty<Guid>()),
			CancellationToken);

		result.Organizations.Should().BeEmpty();
	}

	public sealed class GetOrganizationsByIdsScenario : ReadScenario
	{
		public GetOrganizationsByIdsScenario() => RegisterBasicContext<UserRelationsTestContext>();

		protected override IEnumerable<object> Seed() => [];
	}
}
