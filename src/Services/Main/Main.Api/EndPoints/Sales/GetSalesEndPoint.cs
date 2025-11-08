using Carter;
using Core.Models;
using Main.Application.Handlers.Sales.GetSales;
using Main.Core.Dtos.Amw.Sales;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Sales;

public class GetSalesRequest
{
    [FromQuery(Name = "rangeStartDate")] public DateTime RangeStartDate { get; set; }
    [FromQuery(Name = "rangeEndDate")] public DateTime RangeEndDate { get; set; }
    [FromQuery(Name = "page")] public int Page { get; set; }
    [FromQuery(Name = "limit")] public int Limit { get; set; }
    [FromQuery(Name = "buyerId")] public Guid? BuyerId { get; set; }
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
                var pagination = new PaginationModel(request.Page, request.Limit);
                var query = new GetSalesQuery(request.RangeStartDate, request.RangeEndDate, pagination,
                    request.BuyerId, request.CurrencyId, request.SortBy, request.SearchTerm);
                var result = await sender.Send(query, token);
                var response = result.Adapt<GetSalesResponse>();
                return Results.Ok(response);
            }).WithTags("Sales")
            .WithDescription("Получение списка продаж")
            .WithDisplayName("Получение продаж");
    }
}