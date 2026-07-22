using Abstractions.Models;
using Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Organizations.GetOrganizationMembers;
using Main.Application.Static;
using Main.Entities.Organization;
using Main.Enums.Organization;
using Tests.DataBuilders.Organization;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Organizations;

public class GetOrganizationMembersTests : IntegrationTest
{
    public GetOrganizationMembersTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<UsersTestContext>();
    }

    [Fact]
    public async Task GetOrganizationMembers_ReturnsUsersWithOrganizationAndRole()
    {
        var users = GetContext<UsersTestContext>().Users.ToArray();
        var organization = await CreateOrganization(users.Select(x => x.Id).ToArray());

        var result = await Mediator.Send(CreateQuery(organization.Id));

        result.Members.Should().HaveCount(3);
        result.Members.Should().OnlyContain(x => x.OrganizationId == organization.Id);
        result.Members.Should().Contain(x =>
            x.User.Id == users[0].Id && x.Role == OrganizationRole.Owner);
        result.Members.Should().Contain(x =>
            x.User.Id == users[1].Id && x.Role == OrganizationRole.Admin);
        result.Members.Should().Contain(x =>
            x.User.Id == users[2].Id && x.Role == OrganizationRole.Member);
    }

    [Fact]
    public async Task GetOrganizationMembers_WithPagination_ReturnsRequestedPage()
    {
        var users = GetContext<UsersTestContext>().Users.ToArray();
        var organization = await CreateOrganization(users.Select(x => x.Id).ToArray());

        var firstPage = await Mediator.Send(CreateQuery(organization.Id, page: 0, size: 2));
        var secondPage = await Mediator.Send(CreateQuery(organization.Id, page: 1, size: 2));

        firstPage.Members.Should().HaveCount(2);
        secondPage.Members.Should().ContainSingle();
        firstPage.Members.Select(x => x.User.Id)
            .Should().NotIntersectWith(secondPage.Members.Select(x => x.User.Id));
    }

    [Fact]
    public async Task GetOrganizationMembers_InvalidPagination_ThrowsValidationException()
    {
        var query = CreateQuery(Guid.NewGuid(), page: -1);

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(query));
    }

    [Fact]
    public async Task GetOrganizationMembers_MissingOrganization_ThrowsDbValidationException()
    {
        var query = CreateQuery(Guid.NewGuid());

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(query));

        exception.Failures.Should().Contain(x =>
            x.ErrorName == ApplicationErrors.OrganizationsNotFound);
    }

    private static GetOrganizationMembersQuery CreateQuery(
        Guid organizationId,
        int page = 0,
        int size = 20)
    {
        return new GetOrganizationMembersQuery(
            organizationId,
            new Pagination(page, size));
    }

    private async Task<Organization> CreateOrganization(Guid[] userIds)
    {
        return await new OrganizationBuilder(Faker)
            .WithOwnerId(userIds[0])
            .WithName("Members organization")
            .WithMember(userIds[1], OrganizationRole.Admin)
            .WithMember(userIds[2])
            .BuildAndAddToDb(Context);
    }
}
