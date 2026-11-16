using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;
using Extensions;

namespace Main.Entities.Product.Product;

public class Characteristic : Entity<Characteristic, int>, ILinqEntity<Characteristic, int>
{

	private Characteristic() { }

	private Characteristic(string name) => SetName(name);

	[Validate]
	public int Id { get; private set; }

	public string Name { get; private set; } = null!;

	[Validate]
	public string NormalizedName { get; private set; } = null!;

	public static Expression<Func<Characteristic, int>> GetKeySelector() => x => x.Id;
	public static Expression<Func<Characteristic, bool>> GetEqualityExpression(int key) => x => x.Id == key;

	public static Characteristic Create(string name) => new(name);

	public void SetName(string name)
	{
		var value = name
			.TrimSafe()
			.EnsureNotNullOrWhiteSpace(CharacteristicNameRequiredMessage.Instance)
			.EnsureMaxLength(128, ArticleCharacteristicNameMaxLengthMessage.Instance);

		var normalizedName = NormalizeName(value)
			.EnsureNotNullOrWhiteSpace(CharacteristicNormalizedNameRequiredMessage.Instance)
			.EnsureMaxLength(128, CharacteristicNormalizedNameMaxLengthMessage.Instance);

		Name = value;
		NormalizedName = normalizedName;
	}

	public static string NormalizeName(string name) => name.ToSlug().ToLowerInvariant();

	public override int GetId() => Id;
}
