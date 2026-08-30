using FluentAssertions;
using Main.Application.Handlers.Organizations;
using Main.Entities.Exceptions;
using Main.Entities.Organization;
using Main.Enums.Organization;
using Tests.DataBuilders.Organization;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Organizations;

public sealed class GetOrganizationTests : IntegrationTest
{
	public GetOrganizationTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<UsersTestContext>();
	}

	[Fact]
	public async Task GetOrganization_ById_ReturnsOrganization()
	{
		var organization = await CreateOrganization();
		var owner = GetContext<UsersTestContext>().Users.First();

		var result = await Mediator.Send(new GetOrganizationQuery(organization.Id));

		result.Organization.Id.Should().Be(organization.Id);
		result.Organization.Name.Should().Be(organization.Name);
		result.Organization.SystemName.Should().Be(organization.SystemName);
		result.Organization.Type.Should().Be(OrganizationType.Business);
		result.Organization.Owner.OrganizationId.Should().Be(organization.Id);
		result.Organization.Owner.Role.Should().Be(OrganizationRole.Owner);
		result.Organization.Owner.User.Id.Should().Be(owner.Id);
	}

	[Fact]
	public async Task GetOrganization_BySystemName_NormalizesAndReturnsOrganization()
	{
		var organization = await CreateOrganization();

		var result = await Mediator.Send(
			new GetOrganizationQuery($"  {organization.SystemName.ToUpperInvariant()}  "));

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
		var owner = GetContext<UsersTestContext>().Users.First();
		var organization = Organization.CreateSystem(Guid.NewGuid(), owner.Id);
		await Context.AddAsync(organization);
		await Context.SaveChangesAsync();
		Context.ChangeTracker.Clear();

		var act = () => Mediator.Send(new GetOrganizationQuery(organization.Id));

		await act.Should().ThrowAsync<OrganizationNotFoundException>();
	}

	private async Task<Organization> CreateOrganization()
	{
		var owner = GetContext<UsersTestContext>().Users.First();
		var organization = await new OrganizationBuilder(Faker)
			.WithOwnerId(owner.Id)
			.WithName("Requested organization")
			.BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();
		return organization;
	}
}
