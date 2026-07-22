using Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Organizations.AddOrganizationMember;
using Main.Application.Handlers.Organizations.ChangeOrganizationMemberRole;
using Main.Application.Handlers.Organizations.CreateOrganization;
using Main.Application.Handlers.Organizations.RemoveOrganizationMember;
using Main.Application.Static;
using Main.Entities.Organization;
using Main.Enums.Organization;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders.Organization;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Organizations;

public class OrganizationManagementTests : IntegrationTest
{
    public OrganizationManagementTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<UsersTestContext>();
    }

    [Fact]
    public async Task CreateOrganization_CreatesBusinessOrganizationWithOwner()
    {
        var owner = Users[0];
        var command = new CreateOrganizationCommand(
            owner.Id,
            "New organization",
            $"new-organization-{Guid.NewGuid():N}");

        var result = await Mediator.Send(command);

        result.Organization.Name.Should().Be(command.Name);
        result.Organization.SystemName.Should().Be(command.SystemName);
        result.Organization.Type.Should().Be(OrganizationType.Business);
        result.Organization.Owner.OrganizationId.Should().Be(result.Organization.Id);
        result.Organization.Owner.Role.Should().Be(OrganizationRole.Owner);
        result.Organization.Owner.User.Id.Should().Be(owner.Id);

        var organization = await Context.Organizations
            .Include(x => x.Members)
            .AsNoTracking()
            .SingleAsync(x => x.Id == result.Organization.Id);
        organization.Members.Should().ContainSingle(x =>
            x.UserId == owner.Id && x.Role == OrganizationRole.Owner);
    }

    [Fact]
    public async Task CreateOrganization_MissingOwner_ThrowsDbValidationException()
    {
        var command = new CreateOrganizationCommand(
            Guid.NewGuid(),
            "New organization",
            $"new-organization-{Guid.NewGuid():N}");

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));

        exception.Failures.Should().Contain(x => x.ErrorName == ApplicationErrors.UsersNotFound);
    }

    [Fact]
    public async Task CreateOrganization_DuplicateSystemName_ThrowsDbValidationException()
    {
        var existing = await CreateOrganization();
        var command = new CreateOrganizationCommand(
            Users[1].Id,
            "Another organization",
            existing.SystemName);

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));

        exception.Failures.Should().Contain(x =>
            x.ErrorName == ApplicationErrors.OrganizationSystemNameAlreadyTaken);
    }

    [Fact]
    public async Task AddOrganizationMember_AddsUserWithRole()
    {
        var organization = await CreateOrganization();
        var member = Users[1];

        await Mediator.Send(
            new AddOrganizationMemberCommand(
                organization.Id,
                member.Id,
                OrganizationRole.Manager));

        var organizationMember = await Context.Set<OrganizationMember>()
            .AsNoTracking()
            .SingleAsync(x =>
                x.OrganizationId == organization.Id &&
                x.UserId == member.Id);
        organizationMember.Role.Should().Be(OrganizationRole.Manager);
    }

    [Fact]
    public async Task AddOrganizationMember_DuplicateMember_ThrowsDbValidationException()
    {
        var organization = await CreateOrganization();
        var command = new AddOrganizationMemberCommand(
            organization.Id,
            Users[0].Id,
            OrganizationRole.Member);

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));

        exception.Failures.Should().Contain(x =>
            x.ErrorName == ApplicationErrors.OrganizationMemberAlreadyExists);
    }

    [Fact]
    public async Task ChangeOrganizationMemberRole_ChangesRole()
    {
        var organization = await CreateOrganization(Users[1].Id);

        await Mediator.Send(
            new ChangeOrganizationMemberRoleCommand(
                organization.Id,
                Users[1].Id,
                OrganizationRole.Admin));

        var member = await Context.Set<OrganizationMember>()
            .AsNoTracking()
            .SingleAsync(x =>
                x.OrganizationId == organization.Id &&
                x.UserId == Users[1].Id);
        member.Role.Should().Be(OrganizationRole.Admin);
    }

    [Fact]
    public async Task RemoveOrganizationMember_RemovesMember()
    {
        var organization = await CreateOrganization(Users[1].Id);

        await Mediator.Send(
            new RemoveOrganizationMemberCommand(organization.Id, Users[1].Id));

        var exists = await Context.Set<OrganizationMember>()
            .AsNoTracking()
            .AnyAsync(x =>
                x.OrganizationId == organization.Id &&
                x.UserId == Users[1].Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveOrganizationMember_Owner_ThrowsInvalidInputException()
    {
        var organization = await CreateOrganization();

        var exception = await Assert.ThrowsAsync<InvalidInputException>(() =>
            Mediator.Send(
                new RemoveOrganizationMemberCommand(organization.Id, Users[0].Id)));

        exception.MessageKey.Should().Be("organization.owner.cannot.be.removed");
    }

    private IReadOnlyList<Main.Entities.User.User> Users =>
        GetContext<UsersTestContext>().Users.ToArray();

    private async Task<Organization> CreateOrganization(Guid? memberId = null)
    {
        var builder = new OrganizationBuilder(Faker)
            .WithOwnerId(Users[0].Id)
            .WithName("Managed organization");

        if (memberId.HasValue)
            builder.WithMember(memberId.Value);

        var organization = await builder.BuildAndAddToDb(Context);
        Context.ChangeTracker.Clear();

        return organization;
    }
}
