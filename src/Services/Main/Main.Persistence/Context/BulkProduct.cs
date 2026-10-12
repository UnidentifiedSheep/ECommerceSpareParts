using Main.Entities.Product;

namespace Main.Persistence.Context;

// Flat representation of public.products used only by EFCore.BulkExtensions.
internal sealed class BulkProduct
{
	public int Id { get; init; }
	public int? PairId { get; init; }
	public required string Sku { get; init; }
	public required string NormalizedSku { get; init; }
	public required string Name { get; init; }
	public int Stock { get; init; }
	public string? Description { get; init; }
	public int? PackingUnit { get; init; }
	public int ProducerId { get; init; }
	public string? Indicator { get; init; }
	public int? CategoryId { get; init; }
	public int? ProductGroupId { get; init; }
	public long Popularity { get; init; }
	public uint RowVersion { get; init; }
	public DateTime CreatedAt { get; init; }
	public DateTime UpdatedAt { get; init; }
	public Guid? WhoCreated { get; init; }
	public Guid? WhoUpdated { get; init; }

	public static BulkProduct FromProduct(Product product) => new()
	{
		Id = product.Id,
		PairId = product.PairId,
		Sku = product.Sku.Value,
		NormalizedSku = product.Sku.NormalizedValue,
		Name = product.Name.Value,
		Stock = product.Stock.Value,
		Description = product.Description,
		PackingUnit = product.PackingUnit,
		ProducerId = product.ProducerId,
		Indicator = product.Indicator?.Value,
		CategoryId = product.CategoryId,
		ProductGroupId = product.ProductGroupId,
		Popularity = product.Popularity,
		CreatedAt = product.CreatedAt,
		UpdatedAt = product.UpdatedAt,
		WhoCreated = product.WhoCreated,
		WhoUpdated = product.WhoUpdated
	};
}
