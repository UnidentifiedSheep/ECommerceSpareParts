using Contracts.Products;
using Extensions;
using System.Linq.Expressions;
using Search.Entities;

namespace Search.Application.Models.ProductUpdates;

public sealed record ProductWeightUpdate : ProductFieldUpdate
{
	public ProductWeight? Weight { get; init; }

	public static ProductWeightUpdate From(ProductWeightSnapshot product, DateTime updatedAt)
	{
		var weight = product.Weight;
		return new ProductWeightUpdate
		{
			ProductId = product.ProductId,
			UpdatedAt = updatedAt,
			Weight = weight is null ? null : new ProductWeight
			{
				Value = weight.Value,
				Unit = weight.Unit,
				WeightKg = weight.Value.ToKg(weight.Unit)
			}
		};
	}

	public override Expression<Func<Product, DateTime?>> GetUpdatedAtField()
		=> product => product.WeightUpdatedAt;
}
