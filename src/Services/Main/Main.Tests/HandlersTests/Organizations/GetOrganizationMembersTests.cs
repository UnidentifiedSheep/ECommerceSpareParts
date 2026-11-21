using Application.Common.Models;
using FluentAssertions;
using Main.Entities.Organization.Enums.Organization;
using Main.Entities.Organization.Exceptions;
using Main.Entities.Organization.Organization;
using Main.Modules.Organization.Application.Queries.Organizations.GetOrganizationMembers;
using Tests.Abstractions.Test;
using Tests.DataBuilders.Organization;
using Tests.TestContexts;

namespace Tests.HandlersTests.Organizations;

public class GetOrganizationMembersTests(
	ReadScenarioFixture<GetOrganizationMembersTests.OrganizationMembersScenario> fixture)
	: ReadScenarioTest<GetOrganizationMembersTests.OrganizationMembersScenario>(fixture)
{
	[Fact]
	public async Task GetOrganizationMembers_ReturnsUsersWithOrganizationAndRole()
	{
		var users = Scenario.Get<UsersTestContext>().Users.ToArray();
		var organization = Scenario.Organization;

		var result = await Mediator.Send(CreateQuery(organization.Id), CancellationToken);

		result.Members.Should().HaveCount(3);
		result.Members.Should().OnlyContain(x => x.OrganizationId == organization.Id);
		result.Members.Should().Contain(x => x.UserId == users[0].Id && x.Role == OrganizationRole.Owner);
		result.Members.Should().Contain(x => x.UserId == users[1].Id && x.Role == OrganizationRole.Admin);
		result.Members.Should().Contain(x => x.UserId == users[2].Id && x.Role == OrganizationRole.Member);
	}

	[Fact]
	public async Task GetOrganizationMembers_WithPagination_ReturnsRequestedPage()
	{
		var organization = Scenario.Organization;

		var firstPage = await Mediator.Send(
			CreateQuery(
				organization.Id,
				0,
				2),
			CancellationToken);
		var secondPage = await Mediator.Send(
			CreateQuery(
				organization.Id,
				1,
				2),
			CancellationToken);

		firstPage.Members.Should().HaveCount(2);
		secondPage.Members.Should().ContainSingle();
		firstPage
			.Members
			.Select(x => x.UserId)
			.Should()
			.NotIntersectWith(secondPage.Members.Select(x => x.UserId));
	}

	[Fact]
	public async Task GetOrganizationMembers_MissingOrganization_ThrowsDbValidationException()
	{
		var query = CreateQuery(Guid.NewGuid());

		var exception =
			await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(query, CancellationToken));

		exception.Failures.Should().Contain(x => x.Error is OrganizationNotFoundException);
	}

	private static GetOrganizationMembersQuery CreateQuery(
		Guid organizationId,
		int page = 0,
		int size = 20)
		=> new(organizationId, new Pagination(page, size));

	public sealed class OrganizationMembersScenario : ReadScenario
	{
		public OrganizationMembersScenario() => RegisterBasicContext<UsersTestContext>();

		public Organization Organization { get; private set; } = null!;

		protected override IEnumerable<object> Seed()
		{
			var users = Get<UsersTestContext>().Users.ToArray();
			Organization = new OrganizationBuilder(Faker)
				.WithOwnerId(users[0].Id)
				.WithName("Members organization")
				.WithMember(users[1].Id, OrganizationRole.Admin)
				.WithMember(users[2].Id)
				.Build();
			return [Organization];
		}
	}
}

public class
	GetOrganizationMembersValidationTests : ValidationTest<GetOrganizationMembersValidation,
	GetOrganizationMembersQuery>
{
	[Fact]
	public async Task GetOrganizationMembers_InvalidPagination_FailsValidation()
	{
		var query = new GetOrganizationMembersQuery(Guid.NewGuid(), new Pagination(-1, 20));

		var result = await ValidateAsync(query);
		result.ShouldHaveValidationErrorFor("Pagination.Page");
	}
}
