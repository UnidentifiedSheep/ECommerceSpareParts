using Abstractions.Models;
using Application.Common.Models;
using Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Organizations.GetOrganizations;
using Main.Application.Static;
using Main.Entities.Organization;
using Main.Enums.Organization;
using Main.Enums;
using Tests.DataBuilders.Organization;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Organizations;

public class GetOrganizationsTests : IntegrationTest
{
	public GetOrganizationsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<UsersTestContext>();
	}

	[Fact]
	public async Task GetOrganizations_ByName_ReturnsMatchingOrganization()
	{
		var expected = await CreateOrganization("Northern Parts", "northern-parts");
		await CreateOrganization("Southern Logistics", "southern-logistics");

		var result = await Mediator.Send(CreateQuery("nOrThErN"), CancellationToken);

		result.Organizations.Should().ContainSingle();
		result.Organizations[0].Id.Should().Be(expected.Id);
		result.Organizations[0].Owner.OrganizationId.Should().Be(expected.Id);
		result.Organizations[0].Owner.Role.Should().Be(OrganizationRole.Owner);
		result.Organizations[0].Owner.User.Id.Should().Be(GetContext<UsersTestContext>().Users.First().Id);
	}

	[Fact]
	public async Task GetOrganizations_BySystemName_ReturnsMatchingOrganization()
	{
		var expected = await CreateOrganization("First organization", "unique-supplier-code");
		await CreateOrganization("Unrelated warehouse", "warehouse-code");

		var result = await Mediator.Send(CreateQuery("UNIQUE-SUPPLIER"), CancellationToken);

		result.Organizations.Should().ContainSingle();
		result.Organizations[0].Id.Should().Be(expected.Id);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("Organization contact")]
	public async Task GetOrganizations_ByMemberProfile_ReturnsMemberOrganizations(string? description)
	{
		var users = GetContext<UsersTestContext>().Users.ToArray();
		var member = users[1];
		member.SetUserInfo(
			"Distinctive",
			"Membername",
			description);

		var expected = await new OrganizationBuilder(Faker)
			.WithOwnerId(users[0].Id)
			.WithName("Member organization")
			.WithMember(member.Id)
			.BuildAndAddToDb(Context);

		var result = await Mediator.Send(CreateQuery("Distinctive Membername"), CancellationToken);

		result.Organizations.Should().Contain(x => x.Id == expected.Id);
	}

	[Theory]
	[InlineData("preview-member-login")]
	[InlineData("organization.contact@contacts.example")]
	[InlineData("+7 (999) 123-45-67")]
	public async Task GetOrganizations_ByMemberContactWithoutProfile_ReturnsMatchingOrganization(string term)
	{
		var member = new UserBuilder(Faker).WithUserName("preview-member-login")
			.WithEmail("organization.contact@contacts.example").Build();
		member.AddUserPhone("+7 (999) 123-45-67", PhoneType.Unknown, false, false);
		Context.Add(member);
		var expected = await CreateOrganization("First warehouse", "first-warehouse");
		expected.AddMember(member.Id, OrganizationRole.Member);
		await Context.SaveChangesAsync(CancellationToken);
		var unrelated = await CreateOrganization("Second warehouse", "second-warehouse");

		var result = await Mediator.Send(CreateQuery(term, ids: [expected.Id, unrelated.Id]), CancellationToken);

		result.Organizations.Select(organization => organization.Id).Should().Equal(expected.Id);
	}

	[Fact]
	public async Task GetOrganizations_SearchRanksByBestMemberAndPaginatesWithoutDuplicates()
	{
		var strongMember = await new UserBuilder(Faker).WithUserName("organization-strong-member")
			.WithUserInfo("Космонавтов", "Петров", null).BuildAndAddToDb(Context);
		var weakMember = await new UserBuilder(Faker).WithUserName("organization-weak-member")
			.WithUserInfo("Николай", "Иванов", "Космонавтов").BuildAndAddToDb(Context);
		var first = await CreateOrganization("Alpha warehouse", "alpha-warehouse");
		var second = await CreateOrganization("Beta warehouse", "beta-warehouse");
		var weak = await CreateOrganization("Gamma warehouse", "gamma-warehouse");
		var unrelated = await CreateOrganization("Delta warehouse", "delta-warehouse");
		first.AddMember(strongMember.Id, OrganizationRole.Member);
		second.AddMember(strongMember.Id, OrganizationRole.Member);
		weak.AddMember(weakMember.Id, OrganizationRole.Member);
		var strongOrganizations = new[] { first, second }.OrderBy(organization => organization.Id).ToArray();
		strongOrganizations[1].AddMember(weakMember.Id, OrganizationRole.Member);
		await Context.SaveChangesAsync(CancellationToken);

		var ids = new[] { first.Id, second.Id, weak.Id, unrelated.Id };
		var organizations = new List<Guid>();
		for (var page = 0; page < 3; page++)
		{
			var result = await Mediator.Send(CreateQuery("космонавтов", ids: ids, page: page, size: 1), CancellationToken);
			organizations.Add(result.Organizations.Should().ContainSingle().Which.Id);
		}

		organizations.Should().Equal(strongOrganizations[0].Id, strongOrganizations[1].Id, weak.Id);
		var last = await Mediator.Send(CreateQuery("космонавтов", ids: ids, page: 3, size: 1), CancellationToken);
		last.Organizations.Should().BeEmpty();

		var customSort = await Mediator.Send(
			CreateQuery("космонавтов", ids: ids, sortBy: ["name_desc"]), CancellationToken);
		customSort.Organizations.Select(organization => organization.Id).Should().Equal(weak.Id, second.Id, first.Id);
	}

	[Fact]
	public async Task GetOrganizations_ByIds_ReturnsOnlyRequestedOrganizations()
	{
		var expected = await CreateOrganization("Requested organization", "requested-organization");
		await CreateOrganization("Skipped organization", "skipped-organization");

		var result = await Mediator.Send(CreateQuery(ids: [expected.Id]), CancellationToken);

		result.Organizations.Should().ContainSingle();
		result.Organizations[0].Id.Should().Be(expected.Id);
	}

	[Fact]
	public async Task GetOrganizations_ByType_ReturnsOnlyOrganizationsOfRequestedType()
	{
		await CreateOrganization("Business organization", "business-organization");

		var result = await Mediator.Send(CreateQuery(types: [OrganizationType.Business]), CancellationToken);

		result.Organizations.Should().NotBeEmpty();
		result.Organizations.Should().OnlyContain(x => x.Type == OrganizationType.Business);
	}

	[Fact]
	public async Task GetOrganizations_ByUserId_ReturnsOrganizationsWhereUserIsMember()
	{
		var users = GetContext<UsersTestContext>().Users.ToArray();
		var member = users[1];
		var expected = await CreateOrganization("Member organization", "member-organization");
		expected.AddMember(member.Id, OrganizationRole.Member);
		await Context.SaveChangesAsync(CancellationToken);
		var notExpected = await CreateOrganization("Other organization", "other-organization");

		var result = await Mediator.Send(CreateQuery(userId: member.Id), CancellationToken);

		result.Organizations.Should().Contain(x => x.Id == expected.Id);
		result.Organizations.Should().Contain(x => x.Id == member.Id);
		result.Organizations.Should().NotContain(x => x.Id == notExpected.Id);
	}

	[Fact]
	public async Task GetOrganizations_MissingUser_ThrowsDbValidationException()
	{
		var query = CreateQuery(userId: Guid.NewGuid());

		var exception =
			await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(query, CancellationToken));

		exception.Failures.Should().Contain(x => x.ErrorName == ApplicationErrors.UsersNotFound);
	}

	[Fact]
	public async Task GetOrganizations_WithPagination_ReturnsRequestedPageSize()
	{
		await CreateOrganization("First organization", "first-organization");
		await CreateOrganization("Second organization", "second-organization");

		var result = await Mediator.Send(CreateQuery(size: 1), CancellationToken);

		result.Organizations.Should().ContainSingle();
	}

	[Fact]
	public async Task GetOrganizations_WithSortBy_ReturnsOrganizationsInRequestedOrder()
	{
		var first = await CreateOrganization("Alpha organization", "alpha-organization");
		var second = await CreateOrganization("Zulu organization", "zulu-organization");

		var result = await Mediator.Send(
			CreateQuery(ids: [first.Id, second.Id], sortBy: ["name_desc"]),
			CancellationToken);

		result.Organizations.Select(x => x.Id).Should().Equal(second.Id, first.Id);
	}

	[Fact]
	public async Task GetOrganizations_WithMultipleSortFields_AppliesThemInOrder()
	{
		var first = await CreateOrganization("Same organization", "zulu-organization");
		var second = await CreateOrganization("Same organization", "alpha-organization");

		var result = await Mediator.Send(
			CreateQuery(ids: [first.Id, second.Id], sortBy: ["name", "systemName_desc"]),
			CancellationToken);

		result.Organizations.Select(x => x.Id).Should().Equal(first.Id, second.Id);
	}

	[Theory]
	[InlineData("unknown_desc")]
	[InlineData("name_down")]
	public async Task GetOrganizations_WithInvalidSort_ThrowsInvalidInputException(string sortBy)
	{
		var query = CreateQuery(sortBy: [sortBy]);

		await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(query, CancellationToken));
	}

	[Fact]
	public async Task GetOrganizations_InvalidPagination_ThrowsValidationException()
	{
		var query = CreateQuery(page: -1);

		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(query, CancellationToken));
	}

	[Fact]
	public async Task GetOrganizations_ShowHidden_ReturnsAlsoHiddenOrganizations()
	{
		var hiddenOrg = await CreateOrganization(
			"hidden_org",
			"hidden_org",
			true);

		var notHiddenOrg = await CreateOrganization("not_hidden_org", "not_hidden_org");

		var act = async () => await Mediator.Send(CreateQuery(showHidden: true));
		var result = await act.Should().NotThrowAsync();

		result
			.Which
			.Organizations
			.Should()
			.Contain(x => x.Id == hiddenOrg.Id)
			.Which
			.IsHidden
			.Should()
			.BeTrue();

		result.Which.Organizations.Should().Contain(x => x.Id == notHiddenOrg.Id);
	}

	[Fact]
	public async Task GetOrganizations_ShowHiddenFalse_ReturnsWithOutHiddenOrganizations()
	{
		var hiddenOrg = await CreateOrganization(
			"hidden_org",
			"hidden_org",
			true);

		var notHiddenOrg = await CreateOrganization("not_hidden_org", "not_hidden_org");

		var act = async () => await Mediator.Send(CreateQuery(showHidden: false));
		var result = await act.Should().NotThrowAsync();

		result.Which.Organizations.Should().NotContain(x => x.Id == hiddenOrg.Id);
		result.Which.Organizations.Should().Contain(x => x.Id == notHiddenOrg.Id);
	}

	[Fact]
	public async Task GetOrganizations_ByUserId_AppliesSearchFiltersSortingAndPaginationTogether()
	{
		var member = GetContext<UsersTestContext>().Users.ElementAt(1);
		var first = await CreateOrganization("First organization", "first-organization");
		var shared = await CreateOrganization("Shared organization", "shared-organization");
		var other = await CreateOrganization("Other organization", "other-organization");
		first.AddMember(member.Id, OrganizationRole.Member);
		shared.AddMember(member.Id, OrganizationRole.Manager);
		await Context.SaveChangesAsync(CancellationToken);

		var query = CreateQuery(
			searchTerm: "organization",
			userId: member.Id,
			ids: [first.Id, shared.Id, other.Id],
			types: [OrganizationType.Business],
			sortBy: ["name_desc"],
			size: 1);

		var firstPage = await Mediator.Send(query, CancellationToken);
		var secondPage = await Mediator.Send(query with { Pagination = new Pagination(1, 1) }, CancellationToken);
		var lastPage = await Mediator.Send(query with { Pagination = new Pagination(2, 1) }, CancellationToken);

		firstPage.Organizations.Should().ContainSingle().Which.Id.Should().Be(shared.Id);
		secondPage.Organizations.Should().ContainSingle().Which.Id.Should().Be(first.Id);
		lastPage.Organizations.Should().BeEmpty();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task GetOrganizations_ByUserId_ShowHiddenControlsHiddenMemberships(bool showHidden)
	{
		var member = GetContext<UsersTestContext>().Users.ElementAt(1);
		var hidden = await CreateOrganization("Hidden organization", "hidden-organization", true);
		var other = await CreateOrganization("Other hidden organization", "other-hidden-organization", true);
		hidden.AddMember(member.Id, OrganizationRole.Member);
		await Context.SaveChangesAsync(CancellationToken);

		var query = CreateQuery(
			userId: member.Id, ids: [hidden.Id, other.Id], showHidden: showHidden);

		var result = await Mediator.Send(query, CancellationToken);

		if (showHidden)
			result.Organizations.Should().ContainSingle().Which.Id.Should().Be(hidden.Id);
		else
			result.Organizations.Should().BeEmpty();
	}

	[Fact]
	public async Task GetOrganizations_ByUserIdAndIndividualType_ReturnsOnlyOwnOrganization()
	{
		var users = GetContext<UsersTestContext>().Users.ToArray();
		var business = await CreateOrganization("Business organization", "business-organization");
		var query = CreateQuery(
			userId: users[0].Id,
			ids: [users[0].Id, users[1].Id, business.Id],
			types: [OrganizationType.Individual]);

		var result = await Mediator.Send(query, CancellationToken);

		result.Organizations.Should().ContainSingle().Which.Id.Should().Be(users[0].Id);
	}

	private GetOrganizationsQuery CreateQuery(
		string? searchTerm = null,
		Guid? userId = null,
		IReadOnlyCollection<Guid>? ids = null,
		IReadOnlyCollection<OrganizationType>? types = null,
		IReadOnlyCollection<string>? sortBy = null,
		int page = 0,
		int size = 20,
		bool showHidden = false)
	{
		return new GetOrganizationsQuery(
			new Pagination(page, size),
			sortBy ?? [],
			searchTerm,
			userId,
			ids ?? [],
			types ?? [],
			showHidden);
	}

	private async Task<Organization> CreateOrganization(
		string name,
		string systemName,
		bool isHidden = false)
	{
		var owner = GetContext<UsersTestContext>().Users.First();
		return await new OrganizationBuilder(Faker)
			.WithOwnerId(owner.Id)
			.WithName(name)
			.WithIsHidden(isHidden)
			.WithSystemName($"{systemName}-{Guid.NewGuid():N}")
			.BuildAndAddToDb(Context);
	}
}
