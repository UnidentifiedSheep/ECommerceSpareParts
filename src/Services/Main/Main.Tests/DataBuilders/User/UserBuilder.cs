using Bogus;
using Main.Entities.User.ValueObjects;
using Main.Enums;
using Tests.Abstractions;

namespace Tests.DataBuilders.User;

public class UserBuilder(Faker faker) : BuilderBase<Main.Entities.User.User>(faker)
{
	private readonly List<UserEmailData> _emails = [];

	private readonly List<UserPhoneData> _phones = [];

	private readonly List<string> _roles = [];

	public UserName? UserName { get; private set; }

	public string? PasswordHash { get; private set; }

	public bool UserInfoSet { get; private set; }

	public string? Name { get; private set; }

	public string? Surname { get; private set; }

	public string? Description { get; private set; }

	public IReadOnlyList<string> Roles => _roles;

	public UserBuilder WithUserName(UserName userName)
	{
		UserName = userName;
		return this;
	}

	public UserBuilder WithPasswordHash(string passwordHash)
	{
		PasswordHash = passwordHash;
		return this;
	}

	public UserBuilder WithRole(string role)
	{
		_roles.Add(role);
		return this;
	}

	public UserBuilder WithEmail(
		string email,
		EmailType type = EmailType.Unknown,
		bool isPrimary = false,
		bool isConfirmed = true)
	{
		_emails.Add(
			new UserEmailData(
				email,
				type,
				isPrimary,
				isConfirmed));
		return this;
	}

	public UserBuilder WithPhone(
		string phone,
		PhoneType type = PhoneType.Mobile,
		bool isPrimary = false,
		bool isConfirmed = true)
	{
		_phones.Add(new UserPhoneData(phone, type, isPrimary, isConfirmed));
		return this;
	}

	public UserBuilder WithUserInfo()
	{
		UserInfoSet = true;
		return this;
	}

	public UserBuilder WithUserInfo(
		string name,
		string surname,
		string? description)
	{
		Name = name;
		Surname = surname;
		Description = description;
		UserInfoSet = true;
		return this;
	}

	public override Main.Entities.User.User Build()
	{
		var user = Main.Entities.User.User.Create(
			UserName ?? Faker.Lorem.Letter(6),
			PasswordHash ?? Faker.Lorem.Letter(24));

		if (UserInfoSet)
			user.SetUserInfo(
				Name ?? Faker.Lorem.Letter(12),
				Surname ?? Faker.Lorem.Letter(12),
				Description ?? Faker.Lorem.Sentence());

		foreach (var role in Roles)
			user.AddRole(role);
		foreach (var email in _emails)
			user.AddEmail(
				email.Value,
				email.Type,
				email.IsPrimary,
				email.IsConfirmed);

		foreach (var phone in _phones)
			user.AddUserPhone(
				phone.Value,
				phone.Type,
				phone.IsPrimary,
				phone.IsConfirmed);

		return user;
	}

	private sealed record UserPhoneData(
		string Value,
		PhoneType Type,
		bool IsPrimary,
		bool IsConfirmed);

	private sealed record UserEmailData(
		string Value,
		EmailType Type,
		bool IsPrimary,
		bool IsConfirmed);
}
