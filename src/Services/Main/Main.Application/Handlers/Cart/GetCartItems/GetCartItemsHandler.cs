using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Cart;
using Main.Entities.Cart;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Cart.GetCartItems;

public record GetCartItemsQuery(Guid UserId, Pagination Pagination) : IQuery<GetCartItemsResult>;

public record GetCartItemsResult(List<CartItemDto> CartItems);

public class GetCartItemsHandler(
    IReadRepository<Entities.Cart.Cart, (Guid, int)> repository,
    IProjectionProvider<Entities.Cart.Cart, CartItemDto> cartProjection
)
    : IQueryHandler<GetCartItemsQuery, GetCartItemsResult>
{
    public async Task<GetCartItemsResult> Handle(
        GetCartItemsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository
            .Query
            .Where(x => x.UserId == request.UserId)
            .Project(cartProjection)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetCartItemsResult(result);
    }
}
