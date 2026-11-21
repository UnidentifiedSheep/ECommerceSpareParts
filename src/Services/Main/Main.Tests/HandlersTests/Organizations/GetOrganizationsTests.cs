using Application.Common.Models;
using Application.Common.Querying;
using Exceptions;
using FluentAssertions;
using Main.Entities.Balance.Balance;
using Main.Entities.Enums;
using Main.Entities.Exceptions;
using Main.Entities.Organization.Enums.Organization;
using Main.Entities.Organization.Organization;
using Main.Entities.User;
using Main.Modules.Organization.Application.Queries.Organizations.GetOrganizations;
using Tests.Abstractions.Test;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Organization;
using Tests.DataBuilders.User;
using Tests.TestContexts;

namespace Tests.HandlersTests.Organizations;

public class GetOrganizationsTests(ReadScenarioFixture<GetOrganizationsTests.OrganizationsScenario> fixture)
	: ReadScenarioTest<GetOrganizationsTests.OrganizationsScenario>(fixture)
{
	[Fact]
	public async Task GetOrganizations_ByName_ReturnsMatchingOrganization()
	{
		var result = await Mediator.Send(CreateQuery("nOrThErN"), CancellationToken);

		result.Organizations.Should().ContainSingle();
		result.Organizations[0].Id.Should().Be(Scenario.Northern.Id);
		result.Organizations[0].Owner.OrganizationId.Should().Be(Scenario.Northern.Id);
		result.Organizations[0].Owner.Role.Should().Be(OrganizationRole.Owner);
		result.Organizations[0].Owner.UserId.Should().Be(Scenario.Owner.Id);
	}

	[Fact]
	public async Task GetOrganizations_BySystemName_ReturnsMatchingOrganization()
	{
		var result = await Mediator.Send(CreateQuery("UNIQUE-SUPPLIER"), CancellationToken);

		result.Organizations.Should().ContainSingle();
		result.Organizations[0].Id.Should().Be(Scenario.Northern.Id);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task GetOrganizations_ByMemberProfile_ReturnsMemberOrganizations(bool withDescription)
	{
		var expected = withDescription ? Scenario.Beta : Scenario.Alpha;

		var result = await Mediator.Send(CreateQuery("Distinctive Membername"), CancellationToken);

		result.Organizations.Should().Contain(x => x.Id == expected.Id);
	}

	[Theory]
	[InlineData("preview-member-login")]
	[InlineData("organization.contact@contacts.example")]
	[InlineData("+7 (999) 123-45-67")]
	public async Task GetOrganizations_ByMemberContactWithoutProfile_ReturnsMatchingOrganization(string term)
	{
		var result = await Mediator.Send(
			CreateQuery(term, ids: [Scenario.Gamma.Id, Scenario.Northern.Id]),
			CancellationToken);

		result.Organizations.Select(organization => organization.Id).Should().Equal(Scenario.Gamma.Id);
	}

	[Fact]
	public async Task GetOrganizations_SearchRanksByBestMemberAndPaginatesWithoutDuplicates()
	{
		var ids = new[] { Scenario.Alpha.Id, Scenario.Beta.Id, Scenario.Gamma.Id, Scenario.Northern.Id };
		var strong = new[] { Scenario.Alpha, Scenario.Beta }.OrderBy(organization => organization.Id).ToArray();
		var organizations = new List<Guid>();
		for (var page = 0; page < 3; page++)
		{
			var result = await Mediator.Send(
				CreateQuery(
					"космонавтов",
					ids: ids,
					page: page,
					size: 1),
				CancellationToken);
			organizations.Add(result.Organizations.Should().ContainSingle().Which.Id);
		}

		organizations.Should().Equal(strong[0].Id, strong[1].Id, Scenario.Gamma.Id);
		var last = await Mediator.Send(
			CreateQuery(
				"космонавтов",
				ids: ids,
				page: 3,
				size: 1),
			CancellationToken);
		last.Organizations.Should().BeEmpty();

		var customSort = await Mediator.Send(
			CreateQuery(
				"космонавтов",
				ids: ids,
				sortBy: ["name_desc"]),
			CancellationToken);
		customSort.Organizations
			.Select(organization => organization.Id)
			.Should()
			.Equal(Scenario.Gamma.Id, Scenario.Beta.Id, Scenario.Alpha.Id);
	}

	[Fact]
	public async Task GetOrganizations_ByIds_ReturnsOnlyRequestedOrganizations()
	{
		var result = await Mediator.Send(CreateQuery(ids: [Scenario.Alpha.Id]), CancellationToken);

		result.Organizations.Should().ContainSingle();
		result.Organizations[0].Id.Should().Be(Scenario.Alpha.Id);
	}

	[Fact]
	public async Task GetOrganizations_ByType_ReturnsOnlyOrganizationsOfRequestedType()
	{
		var result = await Mediator.Send(CreateQuery(types: [OrganizationType.Business]), CancellationToken);

		result.Organizations.Should().NotBeEmpty();
		result.Organizations.Should().OnlyContain(x => x.Type == OrganizationType.Business);
	}

	[Fact]
	public async Task GetOrganizations_ByUserId_ReturnsOrganizationsWhereUserIsMember()
	{
		var result = await Mediator.Send(CreateQuery(userId: Scenario.Member.Id), CancellationToken);

		result.Organizations.Should().Contain(x => x.Id == Scenario.Alpha.Id);
		result.Organizations.Should().Contain(x => x.Id == Scenario.Member.Id);
		result.Organizations.Should().NotContain(x => x.Id == Scenario.Northern.Id);
	}

	[Fact]
	public async Task GetOrganizations_MissingUser_ThrowsDbValidationException()
	{
		var query = CreateQuery(userId: Guid.NewGuid());

		var exception =
			await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(query, CancellationToken));

		exception.Failures.Should().Contain(x => x.Error is UserNotFoundException);
	}

	[Fact]
	public async Task GetOrganizations_WithPagination_ReturnsRequestedPageSize()
	{
		var result = await Mediator.Send(CreateQuery(size: 1), CancellationToken);

		result.Organizations.Should().ContainSingle();
	}

	[Fact]
	public async Task GetOrganizations_WithSortBy_ReturnsOrganizationsInRequestedOrder()
	{
		var result = await Mediator.Send(
			CreateQuery(ids: [Scenario.Alpha.Id, Scenario.Gamma.Id], sortBy: ["name_desc"]),
			CancellationToken);

		result.Organizations.Select(x => x.Id).Should().Equal(Scenario.Gamma.Id, Scenario.Alpha.Id);
	}

	[Fact]
	public async Task GetOrganizations_WithMultipleSortFields_AppliesThemInOrder()
	{
		var result = await Mediator.Send(
			CreateQuery(ids: [Scenario.Beta.Id, Scenario.BetaTwin.Id], sortBy: ["name", "systemName_desc"]),
			CancellationToken);

		result.Organizations.Select(x => x.Id).Should().Equal(Scenario.Beta.Id, Scenario.BetaTwin.Id);
	}

	[Theory]
	[InlineData("approximateBalance", false)]
	[InlineData("approximateBalance_desc", true)]
	public async Task GetOrganizations_SortByApproximateBalance_TreatsMissingProfileAsZero(
		string sort,
		bool descending)
	{
		var result = await Mediator.Send(
			CreateQuery(ids: [Scenario.Gamma.Id, Scenario.BetaTwin.Id, Scenario.Northern.Id], sortBy: [sort]),
			CancellationToken);

		Guid[] ascending = [Scenario.BetaTwin.Id, Scenario.Northern.Id, Scenario.Gamma.Id];
		result.Organizations.Select(x => x.Id)
			.Should()
			.Equal(descending ? ascending.Reverse() : ascending);
	}

	[Fact]
	public async Task GetOrganizations_SortByApproximateBalanceAndName_AppliesBothInOrderWithPagination()
	{
		var query = CreateQuery(
			ids: [Scenario.Alpha.Id, Scenario.Beta.Id, Scenario.Gamma.Id],
			sortBy: ["approximateBalance_desc", "name"],
			size: 2);

		var firstPage = await Mediator.Send(query, CancellationToken);
		var secondPage = await Mediator.Send(query with { Pagination = new Pagination(1, 2) }, CancellationToken);

		firstPage.Organizations.Select(x => x.Id).Should().Equal(Scenario.Gamma.Id, Scenario.Alpha.Id);
		secondPage.Organizations.Select(x => x.Id).Should().Equal(Scenario.Beta.Id);
	}

	[Theory]
	[InlineData("unknown", null)]
	[InlineData("approximateBalance", "argument")]
	[InlineData("name", "unexpected")]
	public async Task GetOrganizations_WithInvalidSort_ThrowsInvalidInputException(
		string field,
		string? argument)
	{
		var query = CreateQuery(sortBy: [new SortExpression(field, Argument: argument)]);

		await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(query, CancellationToken));
	}

	[Fact]
	public async Task GetOrganizations_ShowHidden_ReturnsAlsoHiddenOrganizations()
	{
		var result = await Mediator.Send(
			CreateQuery(ids: [Scenario.Hidden.Id, Scenario.Alpha.Id], showHidden: true),
			CancellationToken);

		result.Organizations
			.Should()
			.Contain(x => x.Id == Scenario.Hidden.Id)
			.Which
			.IsHidden
			.Should()
			.BeTrue();
		result.Organizations.Should().Contain(x => x.Id == Scenario.Alpha.Id);
	}

	[Fact]
	public async Task GetOrganizations_ShowHiddenFalse_ReturnsWithOutHiddenOrganizations()
	{
		var result = await Mediator.Send(
			CreateQuery(ids: [Scenario.Hidden.Id, Scenario.Alpha.Id], showHidden: false),
			CancellationToken);

		result.Organizations.Should().NotContain(x => x.Id == Scenario.Hidden.Id);
		result.Organizations.Should().Contain(x => x.Id == Scenario.Alpha.Id);
	}

	[Fact]
	public async Task GetOrganizations_ByUserId_AppliesSearchFiltersSortingAndPaginationTogether()
	{
		var query = CreateQuery(
			"organization",
			Scenario.Member.Id,
			[Scenario.Alpha.Id, Scenario.Beta.Id, Scenario.Gamma.Id],
			[OrganizationType.Business],
			["name_desc"],
			size: 1);

		var firstPage = await Mediator.Send(query, CancellationToken);
		var secondPage = await Mediator.Send(query with { Pagination = new Pagination(1, 1) }, CancellationToken);
		var lastPage = await Mediator.Send(query with { Pagination = new Pagination(2, 1) }, CancellationToken);

		firstPage.Organizations.Should().ContainSingle().Which.Id.Should().Be(Scenario.Beta.Id);
		secondPage.Organizations.Should().ContainSingle().Which.Id.Should().Be(Scenario.Alpha.Id);
		lastPage.Organizations.Should().BeEmpty();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task GetOrganizations_ByUserId_ShowHiddenControlsHiddenMemberships(bool showHidden)
	{
		var query = CreateQuery(
			userId: Scenario.Member.Id,
			ids: [Scenario.Hidden.Id, Scenario.HiddenOther.Id],
			showHidden: showHidden);

		var result = await Mediator.Send(query, CancellationToken);

		if (showHidden)
			result.Organizations.Should().ContainSingle().Which.Id.Should().Be(Scenario.Hidden.Id);
		else
			result.Organizations.Should().BeEmpty();
	}

	[Fact]
	public async Task GetOrganizations_ByUserIdAndIndividualType_ReturnsOnlyOwnOrganization()
	{
		var query = CreateQuery(
			userId: Scenario.Owner.Id,
			ids: [Scenario.Owner.Id, Scenario.Member.Id, Scenario.Alpha.Id],
			types: [OrganizationType.Individual]);

		var result = await Mediator.Send(query, CancellationToken);

		result.Organizations.Should().ContainSingle().Which.Id.Should().Be(Scenario.Owner.Id);
	}

	private static GetOrganizationsQuery CreateQuery(
		string? searchTerm = null,
		Guid? userId = null,
		IReadOnlyCollection<Guid>? ids = null,
		IReadOnlyCollection<OrganizationType>? types = null,
		IReadOnlyCollection<SortExpression>? sortBy = null,
		int page = 0,
		int size = 20,
		bool showHidden = false)
		=> new(
			new Pagination(page, size),
			sortBy ?? [],
			searchTerm,
			userId,
			ids ?? [],
			types ?? [],
			showHidden);

	public sealed class OrganizationsScenario : ReadScenario
	{
		public OrganizationsScenario() => RegisterBasicContext<UsersTestContext>();

		public User Owner { get; private set; } = null!;

		public User Member { get; private set; } = null!;

		public Organization Northern { get; private set; } = null!;

		public Organization Alpha { get; private set; } = null!;

		public Organization Beta { get; private set; } = null!;

		public Organization BetaTwin { get; private set; } = null!;

		public Organization Gamma { get; private set; } = null!;

		public Organization Hidden { get; private set; } = null!;

		public Organization HiddenOther { get; private set; } = null!;

		protected override IEnumerable<object> Seed()
		{
			var users = Get<UsersTestContext>().Users.ToArray();
			Owner = users[0];
			Member = users[1];

			var profileMember = new UserBuilder(Faker).WithUserInfo("Distinctive", "Membername", null).Build();
			var describedMember = new UserBuilder(Faker)
				.WithUserInfo("Distinctive", "Membername", "Organization contact")
				.Build();
			var contact = new UserBuilder(Faker).WithUserName("preview-member-login")
				.WithEmail("organization.contact@contacts.example")
				.Build();
			contact.AddUserPhone(
				"+7 (999) 123-45-67",
				PhoneType.Unknown,
				false,
				false);
			var strongMember = new UserBuilder(Faker).WithUserName("organization-strong-member")
				.WithUserInfo("Космонавтов", "Петров", null)
				.Build();
			var weakMember = new UserBuilder(Faker).WithUserName("organization-weak-member")
				.WithUserInfo("Николай", "Иванов", "Космонавтов")
				.Build();

			Northern = CreateOrganization("Northern Parts", "unique-supplier-code");
			Alpha = CreateOrganization("Alpha organization", "alpha-organization");
			Beta = CreateOrganization("Beta organization", "zulu-organization");
			BetaTwin = CreateOrganization("Beta organization", "alpha-organization");
			Gamma = CreateOrganization("Gamma organization", "gamma-organization");
			Hidden = CreateOrganization("Hidden organization", "hidden-organization", true);
			HiddenOther = CreateOrganization("Other hidden organization", "other-hidden-organization", true);

			Alpha.AddMember(Member.Id, OrganizationRole.Member);
			Beta.AddMember(Member.Id, OrganizationRole.Manager);
			Hidden.AddMember(Member.Id, OrganizationRole.Member);
			Alpha.AddMember(profileMember.Id, OrganizationRole.Member);
			Beta.AddMember(describedMember.Id, OrganizationRole.Member);
			Gamma.AddMember(contact.Id, OrganizationRole.Member);
			Alpha.AddMember(strongMember.Id, OrganizationRole.Member);
			Beta.AddMember(strongMember.Id, OrganizationRole.Member);
			Gamma.AddMember(weakMember.Id, OrganizationRole.Member);
			new[] { Alpha, Beta }.OrderBy(organization => organization.Id)
				.Last()
				.AddMember(weakMember.Id, OrganizationRole.Member);

			return
			[
				profileMember, describedMember, contact, strongMember, weakMember,
				Northern, Alpha, Beta, BetaTwin, Gamma, Hidden, HiddenOther,
				CreateFinancialProfile(Alpha, 10m),
				CreateFinancialProfile(Beta, 10m),
				CreateFinancialProfile(Gamma, 99m),
				CreateFinancialProfile(BetaTwin, -10m)
			];
		}

		private Organization CreateOrganization(string name, string systemName, bool isHidden = false)
			=> new OrganizationBuilder(Faker)
				.WithOwnerId(Owner.Id)
				.WithName(name)
				.WithIsHidden(isHidden)
				.WithSystemName($"{systemName}-{Guid.NewGuid():N}")
				.Build();

		private OrganizationFinancialProfile CreateFinancialProfile(Organization organization, decimal balance)
		{
			var profile = new OrganizationFinancialProfileBuilder(Faker)
				.WithOrganizationId(organization.Id)
				.Build();
			profile.SetApproximateBalance(balance);
			return profile;
		}
	}
}

public class
	GetOrganizationsValidationTests : ValidationTest<GetOrganizationsValidation, GetOrganizationsQuery>
{
	[Fact]
	public async Task GetOrganizations_InvalidPagination_FailsValidation()
	{
		var query = new GetOrganizationsQuery(
			new Pagination(-1, 20),
			[],
			null,
			null,
			[],
			[],
			false);

		var result = await ValidateAsync(query);
		result.ShouldHaveValidationErrorFor("Pagination.Page");
	}
}
