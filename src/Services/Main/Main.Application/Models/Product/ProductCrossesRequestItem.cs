namespace Main.Application.Models.Product;

public sealed record ProductCrossesRequestItem
{
    private readonly IReadOnlyList<string> _sortBy;

    public ProductCrossesRequestItem(
        int productId,
        IEnumerable<string>? sortBy)
    {
        ProductId = productId;
        _sortBy = Array.AsReadOnly(sortBy?.ToArray() ?? []);
    }

    public int ProductId { get; }

    public IReadOnlyList<string> SortBy => _sortBy;

    public bool Equals(ProductCrossesRequestItem? other)
    {
        return other is not null &&
               ProductId == other.ProductId &&
               SortBy.SequenceEqual(other.SortBy, StringComparer.Ordinal);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ProductId);

        foreach (var sort in SortBy) hash.Add(sort, StringComparer.Ordinal);

        return hash.ToHashCode();
    }
}
