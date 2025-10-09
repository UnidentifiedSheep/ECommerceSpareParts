using System.Security.Claims;
using Carter;
using Core.Dtos.Amw.Purchase;
using Core.Models;
using Main.Application.Handlers.Purchases.GetPurchase;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Purchase;

public class GetPurchasesRequest
{
    [FromQuery(Name = "rangeStartDate")] public DateTime RangeStartDate { get; set; }
    [FromQuery(Name = "rangeEndDate")] public DateTime RangeEndDate { get; set; }
    [FromQuery(Name = "page")] public int Page { get; set; }
    [FromQuery(Name = "viewCount")] public int ViewCount { get; set; }
    [FromQuery(Name = "supplierId")] public Guid? SupplierId { get; set; }
    [FromQuery(Name = "currencyId")] public int? CurrencyId { get; set; }
    [FromQuery(Name = "sortBy")] public string? SortBy { get; set; }
    [FromQuery(Name = "searchTerm")] public string? SearchTerm { get; set; }
}

public record GetPurchasesResponse(IEnumerable<PurchaseDto> Purchases);

public class GetPurchasesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/purchases/", async (ISender sender, [AsParameters] GetPurchasesRequest request,
                ClaimsPrincipal user, CancellationToken token) =>
            {
                var query = new GetPurchasesQuery(request.RangeStartDate, request.RangeEndDate,
                    new PaginationModel(request.Page, request.ViewCount),
                    request.SupplierId, request.CurrencyId, request.SortBy, request.SearchTerm);
                var result = await sender.Send(query, token);
                var response = result.Adapt<GetPurchasesResponse>();
                return Results.Ok(response);
            }).RequireAuthorization("AMW")
            .WithTags("Purchases")
            .WithDescription("Получение списка покупок")
            .WithDisplayName("Получение покупок");
    }
}