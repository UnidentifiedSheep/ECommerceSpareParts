using Domain.Validation;

namespace Main.Entities.Product.ValueObjects;

public record Name
{
	private Name()
	{
	}

	public Name(string value)
	{
		value = value.Trim();

		value
			.EnsureNotNullOrWhiteSpace(ArticleNameMustNotBeEmptyMessage.Instance)
			.EnsureMinLength(3, ArticleNameMinLength3Message.Instance)
			.EnsureMaxLength(255, ArticleNameMaxLength255Message.Instance);

		Value = char.ToUpperInvariant(value[0]) + value[1..];
	}

	public string Value { get; } = null!;

	public static implicit operator Name(string value) => new(value);

	public static implicit operator string(Name name) => name.Value;
}
