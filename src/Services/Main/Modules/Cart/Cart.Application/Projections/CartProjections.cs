using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Modules.Cart.Application.Dtos.Cart;

namespace Main.Modules.Cart.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class CartItemDtoProjectionProvider : ProjectionProviderBase<Entities.Cart.Cart.Cart, CartItemDto>
{
	public override Expression<Func<Entities.Cart.Cart.Cart, CartItemDto>> Projection { get; } =
		x => new CartItemDto
		{
			Count = x.Count,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			ProductId = x.ProductId
		};
}
