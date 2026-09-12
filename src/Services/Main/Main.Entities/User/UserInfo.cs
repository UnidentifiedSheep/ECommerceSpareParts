using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;
using Main.Entities.DomainEvents.User;

namespace Main.Entities.User;

public class UserInfo : Entity<UserInfo, Guid>, ILinqEntity<UserInfo, Guid>
{
	private UserInfo()
	{
	}

	private UserInfo(
		Guid userId,
		string name,
		string surname,
		string? description)
	{
		UserId = userId;
		SetName(name);
		SetSurname(surname);
		SetDescription(description);
	}

	public Guid UserId { get; }

	public string Name { get; private set; } = null!;

	public string Surname { get; private set; } = null!;

	public string? Description { get; private set; }

	public string SearchColumn { get; private set; } = null!;

	public static Expression<Func<UserInfo, Guid>> GetKeySelector() => x => x.UserId;

	public static Expression<Func<UserInfo, bool>> GetEqualityExpression(Guid key) => x => x.UserId == key;

	internal static UserInfo Create(
		Guid userId,
		string name,
		string surname,
		string? description)
	{
		return new UserInfo(
			userId,
			name,
			surname,
			description);
	}

	public void SetName(string name)
	{
		Name = name
			.Trim()
			.EnsureNotNullOrWhiteSpace(UserNameRequiredMessage.Instance)
			.EnsureMinLength(3, UserNameMinLengthMessage.Instance)
			.Ensure(x => x.All(c => !char.IsSymbol(c)), UserNameNoSpecialCharsMessage.Instance)
			.EnsureMaxLength(30, UserNameMaxLengthMessage.Instance);
		UpdateSearchColumn();
	}

	public void SetSurname(string surname)
	{
		Surname = surname
			.Trim()
			.EnsureNotNullOrWhiteSpace(UserSurnameRequiredMessage.Instance)
			.EnsureMinLength(3, UserSurnameMinLengthMessage.Instance)
			.Ensure(x => x.All(c => !char.IsSymbol(c)), UserSurnameNoSpecialCharsMessage.Instance)
			.EnsureMaxLength(30, UserSurnameMaxLengthMessage.Instance);
		UpdateSearchColumn();
	}

	public void SetDescription(string? description)
	{
		Description = description.NullIfWhiteSpace()?.EnsureMaxLength(300, UserDescriptionMaxLengthMessage.Instance);
		UpdateSearchColumn();
	}

	internal void Update(
		string name,
		string surname,
		string? description)
	{
		SetName(name);
		SetSurname(surname);
		SetDescription(description);
	}

	private void UpdateSearchColumn() => SearchColumn = $"{Name} {Surname} {Description}".ToUpperInvariant();

	public override void OnCreated() => AddDomainEvent(new UserUpdatedDomainEvent(UserId));

	public override void OnUpdated() => AddDomainEvent(new UserUpdatedDomainEvent(UserId));

	public override void OnDeleted() => AddDomainEvent(new UserUpdatedDomainEvent(UserId));

	public override Guid GetId() => UserId;
}
