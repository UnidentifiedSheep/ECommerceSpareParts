using FluentAssertions;
using Main.Entities.Organization.Enums.Organization;
using Main.Entities.Organization.Exceptions;
using Main.Entities.Organization.Organization;
using Main.Modules.Organization.Application.Queries.Organizations;
using Tests.DataBuilders.Organization;
using Tests.TestContexts;

namespace Tests.HandlersTests.Organizations;

public sealed class GetOrganizationTests(ReadScenarioFixture<GetOrganizationTests.OrganizationScenario> fixture)
	: ReadScenarioTest<GetOrganizationTests.OrganizationScenario>(fixture)
{
	[Fact]
	public async Task GetOrganization_ById_ReturnsOrganization()
	{
		var organization = Scenario.Organization;
		var owner = Scenario.Get<UsersTestContext>().Users.First();

		var result = await Mediator.Send(new GetOrganizationQuery(organization.Id), CancellationToken);

		result.Organization.Id.Should().Be(organization.Id);
		result.Organization.Name.Should().Be(organization.Name);
		result.Organization.SystemName.Should().Be(organization.SystemName);
		result.Organization.Type.Should().Be(OrganizationType.Business);
		result.Organization.Owner.OrganizationId.Should().Be(organization.Id);
		result.Organization.Owner.Role.Should().Be(OrganizationRole.Owner);
		result.Organization.Owner.UserId.Should().Be(owner.Id);
	}

	[Fact]
	public async Task GetOrganization_BySystemName_NormalizesAndReturnsOrganization()
	{
		var organization = Scenario.Organization;

		var result = await Mediator.Send(
			new GetOrganizationQuery($"  {organization.SystemName.ToUpperInvariant()}  "),
			CancellationToken);

		result.Organization.Id.Should().Be(organization.Id);
	}

	[Fact]
	public async Task GetOrganization_MissingId_ThrowsNotFoundException()
	{
		var act = () => Mediator.Send(new GetOrganizationQuery(Guid.NewGuid()));

		await act.Should().ThrowAsync<OrganizationNotFoundException>();
	}

	[Fact]
	public async Task GetOrganization_MissingSystemName_ThrowsNotFoundException()
	{
		var act = () => Mediator.Send(new GetOrganizationQuery($"missing-{Guid.NewGuid():N}"));

		await act.Should().ThrowAsync<OrganizationNotFoundException>();
	}

	[Fact]
	public async Task GetOrganization_SystemOrganization_ThrowsNotFoundException()
	{
		var act = () => Mediator.Send(new GetOrganizationQuery(Scenario.SystemOrganization.Id));

		await act.Should().ThrowAsync<OrganizationNotFoundException>();
	}

	public sealed class OrganizationScenario : ReadScenario
	{
		public OrganizationScenario() => RegisterBasicContext<UsersTestContext>();

		public Organization Organization { get; private set; } = null!;

		public Organization SystemOrganization { get; private set; } = null!;

		protected override IEnumerable<object> Seed()
		{
			var owner = Get<UsersTestContext>().Users.First();
			Organization = new OrganizationBuilder(Faker)
				.WithOwnerId(owner.Id)
				.WithName("Requested organization")
				.Build();
			SystemOrganization = Organization.CreateSystem(Guid.NewGuid(), owner.Id);
			return [Organization, SystemOrganization];
		}
	}
}
