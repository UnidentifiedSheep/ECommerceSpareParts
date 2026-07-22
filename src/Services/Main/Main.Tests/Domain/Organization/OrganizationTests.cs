using Exceptions;
using FluentAssertions;
using Main.Enums.Organization;
using OrganizationDomain = Main.Entities.Organization.Organization;

namespace Tests.Domain.Organization;

public class OrganizationTests
{
    [Fact]
    public void CreateIndividual_WithValidData_CreatesOrganizationWithOwner()
    {
        var ownerId = Guid.NewGuid();

        var organization = OrganizationDomain.CreateIndividual(
            "  Test organization  ",
            ownerId);

        organization.Id.Should().Be(ownerId);
        organization.Name.Should().Be("Test organization");
        organization.SystemName.Should().StartWith("individual-");
        organization.Type.Should().Be(OrganizationType.Individual);
        organization.Members.Should().ContainSingle();
        var owner = organization.Members.Single();
        owner.UserId.Should().Be(ownerId);
        owner.OrganizationId.Should().Be(organization.Id);
        owner.Role.Should().Be(OrganizationRole.Owner);
    }

    [Fact]
    public void CreateBusiness_WithValidData_CreatesOrganizationWithOwner()
    {
        var ownerId = Guid.NewGuid();

        var organization = OrganizationDomain.CreateBusiness(
            "  Test organization  ",
            "  test-organization  ",
            ownerId);

        organization.Id.Should().NotBeEmpty();
        organization.Name.Should().Be("Test organization");
        organization.SystemName.Should().Be("test-organization");
        organization.Type.Should().Be(OrganizationType.Business);
        organization.Members.Should().ContainSingle(x =>
            x.UserId == ownerId
            && x.OrganizationId == organization.Id
            && x.Role == OrganizationRole.Owner);
    }

    [Fact]
    public void CreateSystem_UsesConfiguredIdAndSystemOwner()
    {
        var systemId = Guid.NewGuid();

        var organization = OrganizationDomain.CreateSystem(systemId, systemId);

        organization.Id.Should().Be(systemId);
        organization.Name.Should().Be("System");
        organization.SystemName.Should().Be($"system-{systemId:N}");
        organization.Type.Should().Be(OrganizationType.System);
        organization.Members.Should().ContainSingle(x =>
            x.UserId == systemId &&
            x.OrganizationId == systemId &&
            x.Role == OrganizationRole.Owner);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")]
    public void CreateIndividual_WithInvalidName_ThrowsInvalidInputException(string name)
    {
        var action = () => OrganizationDomain.CreateIndividual(
            name,
            Guid.NewGuid());

        var exception = action.Should().Throw<InvalidInputException>().Which;
        exception.MessageKey.Should().Be(
            string.IsNullOrWhiteSpace(name)
                ? "organization.name.required"
                : "organization.name.min.length");
    }

    [Fact]
    public void CreateIndividual_WithNameExceedingMaximumLength_ThrowsInvalidInputException()
    {
        var action = () => OrganizationDomain.CreateIndividual(
            new string('a', 129),
            Guid.NewGuid());

        action.Should().Throw<InvalidInputException>()
            .Which.MessageKey.Should().Be("organization.name.max.length");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateBusiness_WithoutSystemName_ThrowsInvalidInputException(
        string systemName)
    {
        var action = () => OrganizationDomain.CreateBusiness(
            "Test organization",
            systemName,
            Guid.NewGuid());

        action.Should().Throw<InvalidInputException>()
            .Which.MessageKey.Should().Be("organization.system.name.required");
    }

    [Fact]
    public void SetName_WithValidName_TrimsAndChangesName()
    {
        var organization = CreateBusiness();

        organization.SetName("  Updated organization  ");

        organization.Name.Should().Be("Updated organization");
    }

    [Fact]
    public void AddMember_ToBusinessOrganization_AddsMember()
    {
        var organization = CreateBusiness();
        var userId = Guid.NewGuid();

        organization.AddMember(userId, OrganizationRole.Manager);

        organization.Members.Should().ContainSingle(x =>
            x.UserId == userId
            && x.OrganizationId == organization.Id
            && x.Role == OrganizationRole.Manager);
    }

    [Fact]
    public void AddMember_WithExistingUser_ThrowsInvalidInputException()
    {
        var organization = CreateBusiness();
        var userId = Guid.NewGuid();
        organization.AddMember(userId, OrganizationRole.Member);

        var action = () => organization.AddMember(userId, OrganizationRole.Admin);

        action.Should().Throw<InvalidInputException>()
            .Which.MessageKey.Should().Be("organization.member.already.exists");
    }

    [Fact]
    public void AddMember_WithSecondOwner_ThrowsInvalidInputException()
    {
        var organization = CreateBusiness();

        var action = () => organization.AddMember(
            Guid.NewGuid(),
            OrganizationRole.Owner);

        action.Should().Throw<InvalidInputException>()
            .Which.MessageKey.Should().Be("organization.owner.already.exists");
    }

    [Theory]
    [InlineData(OrganizationRole.Admin)]
    [InlineData(OrganizationRole.Manager)]
    [InlineData(OrganizationRole.Member)]
    [InlineData(OrganizationRole.Owner)]
    public void AddMember_ToIndividualOrganization_ThrowsInvalidInputException(
        OrganizationRole role)
    {
        var organization = OrganizationDomain.CreateIndividual(
            "Individual organization",
            Guid.NewGuid());

        var action = () => organization.AddMember(Guid.NewGuid(), role);

        action.Should().Throw<InvalidInputException>()
            .Which.MessageKey.Should().Be("organization.individual.only.owner.allowed");
    }

    [Fact]
    public void RemoveMember_WithExistingNonOwner_RemovesMember()
    {
        var organization = CreateBusiness();
        var memberId = Guid.NewGuid();
        organization.AddMember(memberId, OrganizationRole.Member);

        organization.RemoveMember(memberId);

        organization.Members.Should().NotContain(x => x.UserId == memberId);
    }

    [Fact]
    public void RemoveMember_WithUnknownUser_DoesNothing()
    {
        var organization = CreateBusiness();
        var membersBefore = organization.Members.ToList();

        var action = () => organization.RemoveMember(Guid.NewGuid());

        action.Should().NotThrow();
        organization.Members.Should().Equal(membersBefore);
    }

    [Fact]
    public void RemoveMember_WithOwner_ThrowsInvalidInputException()
    {
        var ownerId = Guid.NewGuid();
        var organization = CreateBusiness(ownerId);

        var action = () => organization.RemoveMember(ownerId);

        action.Should().Throw<InvalidInputException>()
            .Which.MessageKey.Should().Be("organization.owner.cannot.be.removed");
        organization.Members.Should().ContainSingle(x => x.UserId == ownerId);
    }

    [Fact]
    public void ChangeMemberRole_WithExistingMember_ChangesRole()
    {
        var organization = CreateBusiness();
        var memberId = Guid.NewGuid();
        organization.AddMember(memberId, OrganizationRole.Member);

        organization.ChangeMemberRole(memberId, OrganizationRole.Admin);

        organization.Members.Single(x => x.UserId == memberId)
            .Role.Should().Be(OrganizationRole.Admin);
    }

    [Fact]
    public void ChangeMemberRole_WithSameRole_DoesNothing()
    {
        var organization = CreateBusiness();
        var memberId = Guid.NewGuid();
        organization.AddMember(memberId, OrganizationRole.Manager);

        organization.ChangeMemberRole(memberId, OrganizationRole.Manager);

        organization.Members.Single(x => x.UserId == memberId)
            .Role.Should().Be(OrganizationRole.Manager);
    }

    [Fact]
    public void ChangeMemberRole_WithUnknownUser_ThrowsInvalidInputException()
    {
        var organization = CreateBusiness();

        var action = () => organization.ChangeMemberRole(
            Guid.NewGuid(),
            OrganizationRole.Admin);

        action.Should().Throw<InvalidInputException>()
            .Which.MessageKey.Should().Be("organization.member.not.found");
    }

    [Fact]
    public void ChangeMemberRole_ForOwner_ThrowsInvalidInputException()
    {
        var ownerId = Guid.NewGuid();
        var organization = CreateBusiness(ownerId);

        var action = () => organization.ChangeMemberRole(
            ownerId,
            OrganizationRole.Admin);

        action.Should().Throw<InvalidInputException>()
            .Which.MessageKey.Should().Be("organization.owner.role.cannot.be.changed");
        organization.Members.Single(x => x.UserId == ownerId)
            .Role.Should().Be(OrganizationRole.Owner);
    }

    [Fact]
    public void ChangeMemberRole_ToOwner_ThrowsInvalidInputException()
    {
        var organization = CreateBusiness();
        var memberId = Guid.NewGuid();
        organization.AddMember(memberId, OrganizationRole.Admin);

        var action = () => organization.ChangeMemberRole(
            memberId,
            OrganizationRole.Owner);

        action.Should().Throw<InvalidInputException>()
            .Which.MessageKey.Should().Be("organization.owner.already.exists");
        organization.Members.Should().ContainSingle(x =>
            x.Role == OrganizationRole.Owner);
    }

    private static OrganizationDomain CreateBusiness(Guid? ownerId = null)
    {
        return OrganizationDomain.CreateBusiness(
            "Test organization",
            "test-organization",
            ownerId ?? Guid.NewGuid());
    }
}
