using System.Security.Claims;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Enums;
using Mapster;
using MediatR;
using Pricing.Abstractions.Models;
using Pricing.Application.Handlers.Prices.GetDetailedPrices;
using Pricing.Application.Handlers.Prices.GetPrices;

namespace Pricing.Api.EndPoints.Prices;

public record GetDetailedPricesResponse(Dictionary<int, DetailedPriceModel> Prices);

public record GetPricesResponse(Dictionary<int, double> Prices);

public class GetPricesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/prices", async (ISender sender, bool detailed, int currencyId, Guid? buyerId,
            IUserContext user, HttpContext context, CancellationToken cancellationToken) =>
        {
            var articleIds = context.Request.Query["articleId"]
                .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            if (!user.ContainsPermission(nameof(PermissionCodes.PRICES_GET_DETAILED)) && detailed) return Results.Forbid();
            
            if (detailed) return await GetDetailed(sender, articleIds, buyerId, currencyId, cancellationToken);
            return await GetNormal(sender, articleIds, buyerId, currencyId, cancellationToken);
        });
    }

    private async Task<IResult> GetDetailed(ISender sender, IEnumerable<int> articleIds, Guid? buyerId,
        int currencyId, CancellationToken token)
    {
        var query = new GetDetailedPricesQuery(articleIds, currencyId, buyerId);
        var result = await sender.Send(query, token);
        return Results.Ok(result.Adapt<GetDetailedPricesResponse>());
    }

    private async Task<IResult> GetNormal(ISender sender, IEnumerable<int> articleIds, Guid? buyerId,
        int currencyId, CancellationToken token)
    {
        var query = new GetPricesQuery(articleIds, currencyId, buyerId);
        var result = await sender.Send(query, token);
        return Results.Ok(result.Adapt<GetPricesResponse>());
    }
}