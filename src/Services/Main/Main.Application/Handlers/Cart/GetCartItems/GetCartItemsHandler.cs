using Application.Common.Interfaces;
using Core.Models;
using Main.Abstractions.Dtos.Cart;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Cart.GetCartItems;

public record GetCartItemsQuery(Guid UserId, PaginationModel Pagination) : IQuery<GetCartItemsResult>;
public record GetCartItemsResult(List<CartItemDto> CartItems);

public class GetCartItemsHandler(ICartRepository cartRepository) : IQueryHandler<GetCartItemsQuery, GetCartItemsResult>
{
    public async Task<GetCartItemsResult> Handle(GetCartItemsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Pagination.Page;
        var size = request.Pagination.Size;

        var result = await cartRepository
            .GetCartItemsAsync(request.UserId, false, page, size, cancellationToken);
        return new GetCartItemsResult(result.Adapt<List<CartItemDto>>());
    }
}