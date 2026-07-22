using Bogus;
using Main.Enums.Organization;
using Tests.Abstractions;
using OrganizationEntity = Main.Entities.Organization.Organization;

namespace Tests.DataBuilders.Organization;

public class OrganizationBuilder(Faker faker) : BuilderBase<OrganizationEntity>(faker)
{
    private readonly List<(Guid UserId, OrganizationRole Role)> _members = [];

    public Guid? OwnerId { get; private set; }
    public string? Name { get; private set; }
    public string? SystemName { get; private set; }

    public OrganizationBuilder WithOwnerId(Guid ownerId)
    {
        OwnerId = ownerId;
        return this;
    }

    public OrganizationBuilder WithName(string name)
    {
        Name = name;
        return this;
    }

    public OrganizationBuilder WithSystemName(string systemName)
    {
        SystemName = systemName;
        return this;
    }

    public OrganizationBuilder WithMember(
        Guid userId,
        OrganizationRole role = OrganizationRole.Member)
    {
        _members.Add((userId, role));
        return this;
    }

    public override OrganizationEntity Build()
    {
        var organization = OrganizationEntity.CreateBusiness(
            Name ?? $"Organization {Faker.Random.AlphaNumeric(12)}",
            SystemName ?? $"organization-{Guid.NewGuid():N}",
            OwnerId ?? Guid.NewGuid());

        foreach (var (userId, role) in _members)
            organization.AddMember(userId, role);

        return organization;
    }
}
