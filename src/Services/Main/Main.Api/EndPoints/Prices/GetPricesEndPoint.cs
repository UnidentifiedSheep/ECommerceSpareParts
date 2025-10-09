using System.Security.Claims;
using Carter;
using Core.Models;
using Core.StaticFunctions;
using Main.Application.Handlers.Prices.GetDetailedPrices;
using Main.Application.Handlers.Prices.GetPrices;
using Mapster;
using MediatR;
using Security.Extensions;

namespace Main.Api.EndPoints.Prices;

public record GetDetailedPricesResponse(Dictionary<int, DetailedPriceModel> Prices);

public record GetPricesResponse(Dictionary<int, double> Prices);

public class GetPricesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/prices", async (ISender sender, bool detailed, int currencyId, Guid? buyerId,
            ClaimsPrincipal user, HttpContext context, CancellationToken cancellationToken) =>
        {
            var roles = user.GetUserRoles();

            var articleIds = context.Request.Query["articleId"]
                .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            if (!roles.IsAnyMatchInvariant("admin", "moderator", "worker") && detailed) return Results.Unauthorized();

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