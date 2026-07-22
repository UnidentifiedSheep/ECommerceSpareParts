using Abstractions.Models;
using FluentAssertions;
using Main.Application.Handlers.Organizations.GetOrganizations;
using Main.Entities.Organization;
using Main.Enums.Organization;
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

        var result = await Mediator.Send(CreateQuery(searchTerm: "nOrThErN"));

        result.Organizations.Should().ContainSingle();
        result.Organizations[0].Id.Should().Be(expected.Id);
    }

    [Fact]
    public async Task GetOrganizations_BySystemName_ReturnsMatchingOrganization()
    {
        var expected = await CreateOrganization("Supplier One", "unique-supplier-code");
        await CreateOrganization("Supplier Two", "another-supplier-code");

        var result = await Mediator.Send(CreateQuery(searchTerm: "UNIQUE-SUPPLIER"));

        result.Organizations.Should().ContainSingle();
        result.Organizations[0].Id.Should().Be(expected.Id);
    }

    [Fact]
    public async Task GetOrganizations_ByMemberSearchColumn_ReturnsMemberOrganizations()
    {
        var users = GetContext<UsersTestContext>().Users.ToArray();
        var member = users[1];
        member.SetUserInfo("Distinctive", "Membername", "Organization contact");

        var expected = Organization.CreateBusiness(
            "Member organization",
            $"member-organization-{Guid.NewGuid():N}",
            users[0].Id);
        expected.AddMember(member.Id, OrganizationRole.Member);
        Context.Organizations.Add(expected);
        await Context.SaveChangesAsync();

        var result = await Mediator.Send(CreateQuery(searchTerm: "Distinctive Membername"));

        result.Organizations.Should().Contain(x => x.Id == expected.Id);
    }

    [Fact]
    public async Task GetOrganizations_ByIds_ReturnsOnlyRequestedOrganizations()
    {
        var expected = await CreateOrganization("Requested organization", "requested-organization");
        await CreateOrganization("Skipped organization", "skipped-organization");

        var result = await Mediator.Send(CreateQuery(ids: [expected.Id]));

        result.Organizations.Should().ContainSingle();
        result.Organizations[0].Id.Should().Be(expected.Id);
    }

    [Fact]
    public async Task GetOrganizations_ByType_ReturnsOnlyOrganizationsOfRequestedType()
    {
        await CreateOrganization("Business organization", "business-organization");

        var result = await Mediator.Send(
            CreateQuery(types: [OrganizationType.Business]));

        result.Organizations.Should().NotBeEmpty();
        result.Organizations.Should().OnlyContain(x => x.Type == OrganizationType.Business);
    }

    [Fact]
    public async Task GetOrganizations_WithPagination_ReturnsRequestedPageSize()
    {
        await CreateOrganization("First organization", "first-organization");
        await CreateOrganization("Second organization", "second-organization");

        var result = await Mediator.Send(CreateQuery(size: 1));

        result.Organizations.Should().ContainSingle();
    }

    [Fact]
    public async Task GetOrganizations_WithSortBy_ReturnsOrganizationsInRequestedOrder()
    {
        var first = await CreateOrganization("Alpha organization", "alpha-organization");
        var second = await CreateOrganization("Zulu organization", "zulu-organization");

        var result = await Mediator.Send(
            CreateQuery(
                ids: [first.Id, second.Id],
                sortBy: "name_desc"));

        result.Organizations.Select(x => x.Id).Should().Equal(second.Id, first.Id);
    }

    [Fact]
    public async Task GetOrganizations_InvalidPagination_ThrowsValidationException()
    {
        var query = CreateQuery(page: -1);

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(query));
    }

    private GetOrganizationsQuery CreateQuery(
        string? searchTerm = null,
        IReadOnlyCollection<Guid>? ids = null,
        IReadOnlyCollection<OrganizationType>? types = null,
        string? sortBy = null,
        int page = 0,
        int size = 20)
    {
        return new GetOrganizationsQuery(
            new Pagination(page, size),
            sortBy,
            searchTerm,
            ids ?? [],
            types ?? []);
    }

    private async Task<Organization> CreateOrganization(
        string name,
        string systemName)
    {
        var owner = GetContext<UsersTestContext>().Users.First();
        var organization = Organization.CreateBusiness(
            name,
            $"{systemName}-{Guid.NewGuid():N}",
            owner.Id);

        Context.Organizations.Add(organization);
        await Context.SaveChangesAsync();

        return organization;
    }
}
