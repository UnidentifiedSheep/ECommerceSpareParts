using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pricing.Application.Dtos.Price;
using Pricing.Application.Handlers.Pricing;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.PriceCandidates;

namespace Pricing.Api.EndPoints;

public record GetPriceOffersForProductRequest : PaginationQueryModel
{
    [FromQuery(Name = "productId")]
    public int ProductId { get; init; }

    [FromQuery(Name = "currencyId")]
    public int CurrencyId { get; init; }

    [FromQuery(Name = "storageName")]
    public string StorageName { get; init; } = string.Empty;
}

public record GetPriceOffersForProductResponse
{
    [JsonPropertyName("candidates")]
    public required IReadOnlyCollection<CalculatedScoredPriceCandidate> Candidates { get; init; }
}

public class PriceEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var prices = app.MapGroup("/prices")
            .WithTags("Prices");

        prices.MapGet(
                "/offers",
                async (
                    ISender sender,
                    [AsParameters] GetPriceOffersForProductRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetPriceOffersForProductQuery(
                        request.ProductId,
                        request.CurrencyId,
                        request.StorageName,
                        request);

                    var result = await sender.Send(query, cancellationToken);
                    return Results.Ok(
                        new GetPriceOffersForProductResponse
                        {
                            Candidates = result.Candidates
                        });
                })
            .WithName("GetPriceOffersForProduct")
            .WithSummary("Получить ценовые предложения по продукту")
            .WithDescription("Получение ценовых предложений по продукту, валюте и складу")
            .WithDisplayName("Получение ценовых предложений по продукту")
            .Produces<GetPriceOffersForProductResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.PRICES_GET_DETAILED);
    }
}
