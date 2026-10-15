using Search.Enums;
using Contracts.Products;
using Extensions;
using System.Linq.Expressions;
using Search.Entities;

namespace Search.Application.Models.ProductUpdates;

public sealed record ProductCoreUpdate : ProductFieldUpdate
{
	public string? Sku { get; init; }
	public string? NormalizedSku { get; init; }
	public string? Name { get; init; }
	public int ProducerId { get; init; }
	public int? ProductGroupId { get; init; }
	public int Stock { get; init; }
	public string? Indicator { get; init; }
	public ProductReadinessStatus ReadinessStatus { get; init; }

	public static ProductCoreUpdate From(ProductSnapshot product, DateTime updatedAt) => new()
	{
		ProductId = product.Id,
		UpdatedAt = updatedAt,
		Sku = product.Sku,
		NormalizedSku = product.Sku?.OnlyCharacterToLower(),
		Name = product.Name,
		ProducerId = product.ProducerId,
		ProductGroupId = product.ProductGroupId,
		Stock = product.Stock,
		Indicator = product.Indicator,
		ReadinessStatus = product.IsDeleted ? ProductReadinessStatus.Deleted : ProductReadinessStatus.Ready
	};

	public override Expression<Func<Product, DateTime?>> GetUpdatedAtField() => product => product.ProductUpdatedAt;
}
