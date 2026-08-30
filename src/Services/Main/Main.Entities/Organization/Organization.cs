using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;
using Exceptions;
using Main.Entities.Balance;
using Main.Enums.Organization;

namespace Main.Entities.Organization;

public class Organization : AuditableEntity<Organization, Guid>, ILinqEntity<Organization, Guid>
{

	private readonly List<OrganizationBalance> _balances = [];

	private readonly List<OrganizationMember> _members = [];

	private Organization()
	{
	}

	private Organization(
		string systemName,
		string name,
		OrganizationType type,
		Guid ownerId,
		Guid? id = null)
	{
		Id = id ?? Guid.NewGuid();
		Type = type;
		SetSystemName(systemName);
		SetName(name);
		AddMember(ownerId, OrganizationRole.Owner);
	}

	[Validate]
	public Guid Id { get; }

	public OrganizationType Type { get; }

	public string Name { get; private set; } = null!;

	public bool IsHidden { get; private set; }

	[Validate]
	public string SystemName { get; private set; } = null!;

	public IReadOnlyList<OrganizationMember> Members => _members;

	public IReadOnlyList<OrganizationBalance> Balances => _balances;

	public OrganizationFinancialProfile? FinancialProfile { get; private set; }

	public static Expression<Func<Organization, Guid>> GetKeySelector() => x => x.Id;

	public static Expression<Func<Organization, bool>> GetEqualityExpression(Guid key) => x => x.Id == key;

	public static string NormalizeSystemName(string systemName) => systemName.ToLowerInvariant().TrimSafe();

	public static Organization CreateIndividual(string name, Guid ownerId) => new(
		$"individual-{Guid.NewGuid():N}",
		name,
		OrganizationType.Individual,
		ownerId,
		ownerId);

	public static Organization CreateBusiness(
		string name,
		string systemName,
		Guid ownerId) => new(
		systemName,
		name,
		OrganizationType.Business,
		ownerId);

	public static Organization CreateSystem(Guid id, Guid ownerId) => new(
		$"system-{id:N}",
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

		if (role == OrganizationRole.Owner && _members.Any(x => x.Role == OrganizationRole.Owner))
			throw new InvalidInputException("organization.owner.already.exists");

		_members.Add(
			OrganizationMember.Create(
				userId,
				Id,
				role));
	}

	public void RemoveMember(Guid userId)
	{
		var member = _members.FirstOrDefault(x => x.UserId == userId);
		if (member == null)
			return;
		if (member.Role == OrganizationRole.Owner)
			throw new InvalidInputException("organization.owner.cannot.be.removed");

		_members.Remove(member);
	}

	public void ChangeMemberRole(Guid userId, OrganizationRole role)
	{
		var member = _members.FirstOrDefault(x => x.UserId == userId) ??
			throw new InvalidInputException("organization.member.not.found");

		if (member.Role == role)
			return;
		if (member.Role == OrganizationRole.Owner)
			throw new InvalidInputException("organization.owner.role.cannot.be.changed");
		if (role == OrganizationRole.Owner && _members.Any(x => x.Role == OrganizationRole.Owner))
			throw new InvalidInputException("organization.owner.already.exists");

		member.SetRole(role);
	}

	public void Hide() => SetIsHidden(true);

	public void Show() => SetIsHidden(false);

	public void SetIsHidden(bool isHidden) => IsHidden = isHidden;

	public void SetName(string name) => Name = name
		.TrimSafe()
		.EnsureNotNullOrWhiteSpace("organization.name.required")
		.EnsureMaxLength(128, "organization.name.max.length")
		.EnsureMinLength(3, "organization.name.min.length");

	private void SetSystemName(string systemName) => SystemName = NormalizeSystemName(systemName)
		.EnsureNotNullOrWhiteSpace("organization.system.name.required")
		.EnsureMaxLength(128, "organization.system.name.max.length");

	public override Guid GetId() => Id;
}
