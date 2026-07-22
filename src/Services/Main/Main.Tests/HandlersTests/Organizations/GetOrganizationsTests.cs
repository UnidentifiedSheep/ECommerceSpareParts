using Abstractions.Models;
using Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Organizations.GetOrganizations;
using Main.Application.Static;
using Main.Entities.Organization;
using Main.Enums.Organization;
using Tests.DataBuilders.Organization;
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

        var result = await Mediator.Send(CreateQuery(searchTerm: "nOrThErN"));

        result.Organizations.Should().ContainSingle();
        result.Organizations[0].Id.Should().Be(expected.Id);
    }

    [Fact]
    public async Task GetOrganizations_BySystemName_ReturnsMatchingOrganization()
    {
        var expected = await CreateOrganization("First organization", "unique-supplier-code");
        await CreateOrganization("Unrelated warehouse", "warehouse-code");

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

        var expected = await new OrganizationBuilder(Faker)
            .WithOwnerId(users[0].Id)
            .WithName("Member organization")
            .WithMember(member.Id)
            .BuildAndAddToDb(Context);

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
    public async Task GetOrganizations_ByUserId_ReturnsOrganizationsWhereUserIsMember()
    {
        var users = GetContext<UsersTestContext>().Users.ToArray();
        var member = users[1];
        var expected = await CreateOrganization("Member organization", "member-organization");
        expected.AddMember(member.Id, OrganizationRole.Member);
        await Context.SaveChangesAsync();
        var notExpected = await CreateOrganization("Other organization", "other-organization");

        var result = await Mediator.Send(CreateQuery(userId: member.Id));

        result.Organizations.Should().Contain(x => x.Id == expected.Id);
        result.Organizations.Should().Contain(x => x.Id == member.Id);
        result.Organizations.Should().NotContain(x => x.Id == notExpected.Id);
    }

    [Fact]
    public async Task GetOrganizations_MissingUser_ThrowsDbValidationException()
    {
        var query = CreateQuery(userId: Guid.NewGuid());

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(query));

        exception.Failures.Should().Contain(x => x.ErrorName == ApplicationErrors.UsersNotFound);
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
        Guid? userId = null,
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
            userId,
            ids ?? [],
            types ?? []);
    }

    private async Task<Organization> CreateOrganization(
        string name,
        string systemName)
    {
        var owner = GetContext<UsersTestContext>().Users.First();
        return await new OrganizationBuilder(Faker)
            .WithOwnerId(owner.Id)
            .WithName(name)
            .WithSystemName($"{systemName}-{Guid.NewGuid():N}")
            .BuildAndAddToDb(Context);
    }
}
