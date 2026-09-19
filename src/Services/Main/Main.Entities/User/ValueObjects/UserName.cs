using Domain.Validation;

namespace Main.Entities.User.ValueObjects;

public record UserName
{
	private UserName()
	{
	}

	public UserName(string value)
	{
		value.EnsureNotNullOrWhiteSpace(LoginMustNotBeEmptyMessage.Instance);
		value = value.Trim();

		value
			.EnsureMinLength(5, LoginMinLength5Message.Instance)
			.EnsureMaxLength(36, LoginMaxLength36Message.Instance)
			.EnsureNoSpaces(LoginCannotContainSpacesMessage.Instance)
			.Ensure(
				x => !x.Contains('@', StringComparison.InvariantCulture),
				LoginCannotContainAtSignMessage.Instance);

		Value = value;
		NormalizedValue = ToNormalized(Value);
	}

	public string Value { get; } = null!;

	public string NormalizedValue { get; } = null!;

	public static string ToNormalized(string source) => source.Trim().ToUpperInvariant();

	public static implicit operator UserName(string value) => new(value);

	public static implicit operator string(UserName sku) => sku.Value;
}
