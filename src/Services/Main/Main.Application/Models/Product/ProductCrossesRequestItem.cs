namespace Main.Application.Models.Product;

public sealed record ProductCrossesRequestItem
{

	public ProductCrossesRequestItem(int productId, IEnumerable<string>? sortBy)
	{
		ProductId = productId;
		SortBy = Array.AsReadOnly(sortBy?.ToArray() ?? []);
	}

	public int ProductId { get; }

	public IReadOnlyList<string> SortBy { get; }

	public bool Equals(ProductCrossesRequestItem? other)
	{
		return other is not null && ProductId == other.ProductId &&
			SortBy.SequenceEqual(other.SortBy, StringComparer.Ordinal);
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(ProductId);

		foreach (var sort in SortBy)
			hash.Add(sort, StringComparer.Ordinal);

		return hash.ToHashCode();
	}
}
