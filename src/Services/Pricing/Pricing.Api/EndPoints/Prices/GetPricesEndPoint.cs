using Abstractions.Interfaces;
using Abstractions.Interfaces.Services;
using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Enums;
using MediatR;
using Pricing.Abstractions.Models.Pricing;
using Pricing.Application.Handlers.Prices.GetDetailedPrices;

namespace Pricing.Api.EndPoints.Prices;

public record GetDetailedPricesResponse(Dictionary<int, PricingResult?> Prices);

public record GetPricesResponse(Dictionary<int, string?> Prices);

public class GetPricesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/prices", async (ISender sender, bool detailed, int currencyId, Guid? buyerId, IJsonSigner signer,
                IUserContext user, HttpContext context, CancellationToken cancellationToken) =>
            {
                var articleIds = context.Request.Query["articleId"]
                    .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .ToList();

                if (!detailed) return await GetNormal(sender, articleIds, buyerId, currencyId, signer, cancellationToken);
                if (!user.ContainsPermission(nameof(PermissionCodes.PRICES_GET_DETAILED))) return Results.Forbid();
                
                return await GetDetailed(sender, articleIds, buyerId, currencyId, cancellationToken);
            }).WithTags("Prices")
            .WithDescription("Получение цен")
            .WithDisplayName("Получение цен");
    }

    private async Task<IResult> GetDetailed(ISender sender, IEnumerable<int> articleIds, Guid? buyerId,
        int currencyId, CancellationToken token)
    {
        var query = new GetDetailedPricesQuery(articleIds, currencyId, buyerId);
        var result = await sender.Send(query, token);
        return Results.Ok(new GetDetailedPricesResponse(result.Prices));
    }

    private async Task<IResult> GetNormal(ISender sender, IEnumerable<int> articleIds, Guid? buyerId,
        int currencyId, IJsonSigner signer, CancellationToken token)
    {
        var query = new GetDetailedPricesQuery(articleIds, currencyId, buyerId);
        var result = await sender.Send(query, token);
        var dict = result.Prices
            .ToDictionary(x => x.Key, x => x.Value?.FinalPrice == null 
                ? null 
                : signer.Sign(new Timestamped<decimal> { Value = x.Value.FinalPrice }));
        return Results.Ok(new GetPricesResponse(dict));
    }
}