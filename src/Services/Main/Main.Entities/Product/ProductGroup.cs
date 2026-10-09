using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;
using Extensions;

namespace Main.Entities.Product;

public class ProductGroup : Entity<ProductGroup, int>, ILinqEntity<ProductGroup, int>
{
	public int Id { get; private set; }
	public string Name { get; private set; } = null!;

	[Validate]
	public string NormalizedName { get; private set; } = null!;

	private ProductGroup() {}

	private ProductGroup(string name)
	{
		SetName(name);
	}

	public static ProductGroup Create(string name) => new(name);

	public void SetName(string name)
	{
		var value = name
			.TrimSafe()
			.EnsureNotNullOrWhiteSpace(ProductGroupNameRequiredMessage.Instance)
			.EnsureMinLength(3, ProductGroupNameMinLengthMessage.Instance)
			.EnsureMaxLength(256, ProductGroupNameMaxLengthMessage.Instance);

		var normalizedName = NormalizeName(value)
			.EnsureNotNullOrWhiteSpace(ProductGroupNormalizedNameRequiredMessage.Instance)
			.EnsureMaxLength(256, ProductGroupNormalizedNameMaxLengthMessage.Instance);

		Name = value;
		NormalizedName = normalizedName;
	}

	public static string NormalizeName(string name) => name.ToSlug();

	public override int GetId() => Id;
	public static Expression<Func<ProductGroup, int>> GetKeySelector() => x => x.Id;
	public static Expression<Func<ProductGroup, bool>> GetEqualityExpression(int key) => x => x.Id == key;
}
