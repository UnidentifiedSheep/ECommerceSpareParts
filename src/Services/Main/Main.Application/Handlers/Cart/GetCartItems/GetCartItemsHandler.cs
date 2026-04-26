using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Cart;
using Main.Application.Handlers.Projections;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Cart.GetCartItems;

public record GetCartItemsQuery(Guid UserId, Pagination Pagination) : IQuery<GetCartItemsResult>;

public record GetCartItemsResult(List<CartItemDto> CartItems);

public class GetCartItemsHandler(
    IReadRepository<Entities.Cart.Cart, (Guid, int)> repository) 
    : IQueryHandler<GetCartItemsQuery, GetCartItemsResult>
{
    public async Task<GetCartItemsResult> Handle(GetCartItemsQuery request, CancellationToken cancellationToken)
    {
        var result = await repository
            .Query
            .Where(x => x.UserId == request.UserId)
            .AsExpandable()
            .Select(CartProjections.ToCartItemDto)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        
        return new GetCartItemsResult(result);
    }
}