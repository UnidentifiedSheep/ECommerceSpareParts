using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Cart;
using Main.Entities.Cart;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class CartItemDtoProjectionProvider : ProjectionProviderBase<Cart, CartItemDto>
{
	public override Expression<Func<Cart, CartItemDto>> Projection { get; } =
		x => new CartItemDto
		{
			Count = x.Count,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			ProductId = x.ProductId
		};
}
