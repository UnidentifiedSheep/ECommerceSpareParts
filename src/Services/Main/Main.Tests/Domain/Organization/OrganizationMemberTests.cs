using FluentAssertions;
using Main.Entities.Organization;
using Main.Enums.Organization;

namespace Tests.Domain.Organization;

public class OrganizationMemberTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var userId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var member = OrganizationMember.Create(
            userId,
            organizationId,
            OrganizationRole.Manager);

        member.UserId.Should().Be(userId);
        member.OrganizationId.Should().Be(organizationId);
        member.Role.Should().Be(OrganizationRole.Manager);
    }

    [Fact]
    public void SetRole_ChangesRole()
    {
        var member = OrganizationMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            OrganizationRole.Member);

        member.SetRole(OrganizationRole.Admin);

        member.Role.Should().Be(OrganizationRole.Admin);
    }

    [Fact]
    public void GetId_ReturnsCompositeKey()
    {
        var userId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var member = OrganizationMember.Create(
            userId,
            organizationId,
            OrganizationRole.Member);

        var key = member.GetId();

        key.OrganizationId.Should().Be(organizationId);
        key.UserId.Should().Be(userId);
        key.ToArray().Should().Equal(organizationId, userId);
    }

    [Fact]
    public void GetKeySelector_ReturnsMemberKey()
    {
        var member = OrganizationMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            OrganizationRole.Member);

        var key = OrganizationMember.GetKeySelector().Compile()(member);

        key.OrganizationId.Should().Be(member.OrganizationId);
        key.UserId.Should().Be(member.UserId);
    }

    [Fact]
    public void GetEqualityExpression_WithMatchingKey_ReturnsTrue()
    {
        var member = OrganizationMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            OrganizationRole.Member);

        var result = OrganizationMember
            .GetEqualityExpression(member.GetId())
            .Compile()(member);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetEqualityExpression_WithDifferentKey_ReturnsFalse()
    {
        var member = OrganizationMember.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            OrganizationRole.Member);
        var differentKey = new OrganizationMemberKey(
            member.OrganizationId,
            Guid.NewGuid());

        var result = OrganizationMember
            .GetEqualityExpression(differentKey)
            .Compile()(member);

        result.Should().BeFalse();
    }
}
