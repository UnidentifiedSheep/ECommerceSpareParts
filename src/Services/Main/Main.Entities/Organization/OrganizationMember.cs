using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Interfaces;
using Main.Enums.Organization;

namespace Main.Entities.Organization;

public class OrganizationMember : 
    AuditableEntity<OrganizationMember, OrganizationMemberKey>, 
    ILinqEntity<OrganizationMember, OrganizationMemberKey>
{
    [ValidateTuple("PK")]
    public Guid UserId { get; private set; }

    [ValidateTuple("PK")]
    public Guid OrganizationId { get; private set; }
    public OrganizationRole Role { get; private set; }

    public Organization Organization { get; private set; } = null!;
    public User.User User { get; private set; } = null!;
    
    private OrganizationMember() { }

    internal static OrganizationMember Create(
        Guid userId,
        Guid organizationId,
        OrganizationRole role)
        => new()
        {
            UserId = userId,
            OrganizationId = organizationId,
            Role = role
        };
    
    internal void SetRole(OrganizationRole role) => Role = role;
    
    public override OrganizationMemberKey GetId() => new(OrganizationId, UserId);
    public static Expression<Func<OrganizationMember, OrganizationMemberKey>> GetKeySelector()
        => x => new OrganizationMemberKey(x.OrganizationId, x.UserId);
    public static Expression<Func<OrganizationMember, bool>> GetEqualityExpression(OrganizationMemberKey key)
        => x => x.OrganizationId == key.OrganizationId && x.UserId == key.UserId;
}

public readonly struct OrganizationMemberKey(Guid organizationId, Guid userId) : ICompositeKey
{
    public Guid OrganizationId => organizationId;
    public Guid UserId => userId;
    public object[] ToArray() => [organizationId, userId];
}
