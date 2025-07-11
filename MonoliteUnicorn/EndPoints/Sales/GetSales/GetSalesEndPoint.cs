using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MonoliteUnicorn.Dtos.Amw.Sales;

namespace MonoliteUnicorn.EndPoints.Sales.GetSales;

public class GetSalesRequest
{
    [FromQuery(Name = "rangeStartDate")] public DateTime RangeStartDate { get; set; }
    [FromQuery(Name = "rangeEndDate")] public DateTime RangeEndDate { get; set; }
    [FromQuery(Name = "page")] public int Page { get; set; }
    [FromQuery(Name = "viewCount")] public int ViewCount { get; set; }
    [FromQuery(Name = "buyerId")] public string? BuyerId { get; set; }
    [FromQuery(Name = "currencyId")] public int? CurrencyId { get; set; }
    [FromQuery(Name = "sortBy")] public string? SortBy { get; set; }
    [FromQuery(Name = "searchTerm")] public string? SearchTerm { get; set; }
}

public record GetSalesResponse(IEnumerable<SaleDto> Sales);

public class GetSalesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/sales/", async (ISender sender, [AsParameters] GetSalesRequest request, CancellationToken token) =>
            {
                var query = request.Adapt<GetSalesQuery>();
                var result = await sender.Send(query, token);
                var response = result.Adapt<GetSalesResponse>();
                return Results.Ok(response);
            }).RequireAuthorization("AMW")
        .WithGroup("Sales")
        .WithDescription("Получение списка продаж")
        .WithDisplayName("Получение продаж");
    }
}