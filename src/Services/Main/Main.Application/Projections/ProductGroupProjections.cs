using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Application.Dtos.Product;
using Main.Entities.Product;

namespace Main.Application.Projections;

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
