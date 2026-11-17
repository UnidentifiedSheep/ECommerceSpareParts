using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Entities.Product.Product;
using Main.Modules.Product.Application.Dtos.Product;

namespace Main.Modules.Product.Application.Products;

[Lifetime(Lifetime.Singleton)]
public sealed class ProductGroupDtoProjectionProvider : ProjectionProviderBase<ProductGroup, ProductGroupDto>
{
	public override Expression<Func<ProductGroup, ProductGroupDto>> Projection { get; } = group =>
		new ProductGroupDto
		{
			Id = group.Id,
			Name = group.Name,
			NormalizedName = group.NormalizedName
		};
}
