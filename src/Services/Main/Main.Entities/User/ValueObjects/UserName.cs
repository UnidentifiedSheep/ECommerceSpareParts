using Domain.Validation;

namespace Main.Entities.User.ValueObjects;

public record UserName
{
	private UserName()
	{
	}

	public UserName(string value)
	{
		value.EnsureNotNullOrWhiteSpace("login.must.not.be.empty");
		value = value.Trim();

		value
			.EnsureMinLength(5, "login.min.length.5")
			.EnsureMaxLength(36, "login.max.length.36")
			.EnsureNoSpaces("login.cannot.contain.spaces")
			.Ensure(x => !x.Contains('@', StringComparison.InvariantCulture), "login.cannot.contain.at.sign");

		Value = value;
		NormalizedValue = ToNormalized(Value);
	}

	public string Value { get; } = null!;

	public string NormalizedValue { get; } = null!;

	public static string ToNormalized(string source) => source.Trim().ToUpperInvariant();

	public static implicit operator UserName(string value) => new(value);

	public static implicit operator string(UserName sku) => sku.Value;
}
