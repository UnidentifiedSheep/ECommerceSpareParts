using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Exceptions;
using Main.Entities.Balance;
using Main.Enums.Organization;

namespace Main.Entities.Organization;

public class Organization : AuditableEntity<Organization, Guid>, ILinqEntity<Organization, Guid>
{
    public Guid Id { get; private set; }
    public OrganizationType Type { get; private set; }
    public string Name { get; private set; } = null!;
    public string SystemName { get; private set; } = null!;
    
    private readonly List<OrganizationMember> _members = [];
    public IReadOnlyList<OrganizationMember> Members => _members;
    
    private readonly List<OrganizationBalance> _balances = [];
    public IReadOnlyList<OrganizationBalance> Balances => _balances;
    
    public OrganizationFinancialProfile? FinancialProfile { get; private set; }
    
    private Organization() { }

    private Organization(
        string systemName, 
        string name,
        OrganizationType type,
        Guid ownerId,
        Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        Type = type;
        SystemName = systemName.TrimSafe()
            .EnsureNotNullOrWhiteSpace("organization.system.name.required");
        SetName(name);
        AddMember(ownerId, OrganizationRole.Owner);
    }

    public static Organization CreateIndividual(
        string name,
        Guid ownerId)
        => new (
            $"individual-{Guid.NewGuid():N}",
            name, 
            OrganizationType.Individual, 
            ownerId,
            ownerId);
    
    
    public static Organization CreateBusiness(
        string name,
        string systemName,
        Guid ownerId)
        => new Organization(
            systemName,
            name,
            OrganizationType.Business,
            ownerId);

    public static Organization CreateSystem(Guid id, Guid ownerId)
        => new(
            "system",
            "System",
            OrganizationType.System,
            ownerId,
            id);

    public void AddMember(Guid userId, OrganizationRole role)
    {
        if (Type == OrganizationType.Individual && (_members.Count >= 1 || role != OrganizationRole.Owner))
            throw new InvalidInputException("organization.individual.only.owner.allowed");
        
        if (_members.Any(x => x.UserId == userId))
            throw new InvalidInputException("organization.member.already.exists");

        if (role == OrganizationRole.Owner &&
            _members.Any(x => x.Role == OrganizationRole.Owner))
            throw new InvalidInputException("organization.owner.already.exists");

        _members.Add(OrganizationMember.Create(userId, Id, role));
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(x => x.UserId == userId);
        if (member == null) return;
        if (member.Role == OrganizationRole.Owner)
            throw new InvalidInputException("organization.owner.cannot.be.removed");
        
        _members.Remove(member);
    }

    public void ChangeMemberRole(Guid userId, OrganizationRole role)
    {
        var member = _members.FirstOrDefault(x => x.UserId == userId)
            ?? throw new InvalidInputException("organization.member.not.found");

        if (member.Role == role) return;
        if (member.Role == OrganizationRole.Owner)
            throw new InvalidInputException("organization.owner.role.cannot.be.changed");
        if (role == OrganizationRole.Owner &&
            _members.Any(x => x.Role == OrganizationRole.Owner))
            throw new InvalidInputException("organization.owner.already.exists");
        
        member.SetRole(role);
    }
    
    public void SetName(string name)
        => Name = name.TrimSafe()
            .EnsureNotNullOrWhiteSpace("organization.name.required")
            .EnsureMaxLength(128, "organization.name.max.length")
            .EnsureMinLength(3, "organization.name.min.length");
    
    public override Guid GetId() => Id;
    public static Expression<Func<Organization, Guid>> GetKeySelector()
        => x => x.Id;
    public static Expression<Func<Organization, bool>> GetEqualityExpression(Guid key)
        => x => x.Id == key;
}
