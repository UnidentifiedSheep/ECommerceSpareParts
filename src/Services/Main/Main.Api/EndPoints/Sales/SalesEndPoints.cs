using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Sale;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Sales;

public record CreateSaleRequest(
    Guid BuyerId,
    int CurrencyId,
    string StorageName,
    bool SellFromOtherStorages,
    DateTime SaleDateTime,
    IEnumerable<NewSaleContentDto> SaleContent,
    string? Comment,
    decimal? PayedSum,
    string? ConfirmationCode);

public record EditSaleRequest(
    IEnumerable<EditSaleContentDto> EditedContent,
    int CurrencyId,
    DateTime SaleDateTime,
    string? Comment,
    bool SellFromOtherStorages);

public record GetSaleContentResponse(IEnumerable<SaleContentDto> Content);

public record GetSalesResponse(IEnumerable<SaleDto> Sales);

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

public class SalesEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var sales = app.MapGroup("/sales")
            .WithTags("Sales");

        sales.MapPost("/", (
                IUserContext user,
                ISender sender,
                CreateSaleRequest request,
                CancellationToken token) => Results.Ok())
            .WithDescription("Создание новой продажи")
            .WithDisplayName("Создание новой продажи")
            .Produces(200)
            .Produces(401)
            .RequireAnyPermission(PermissionCodes.SALES_CREATE);

        sales.MapDelete("/{saleId}", (
                ISender sender,
                IUserContext user,
                string saleId,
                CancellationToken token) => Results.NoContent())
            .WithDescription("Удаление продажи")
            .WithDisplayName("Удаление продажи")
            .RequireAnyPermission(PermissionCodes.SALES_DELETE);

        sales.MapPut("/{saleId}", (
                ISender sender,
                string saleId,
                EditSaleRequest request,
                IUserContext user,
                CancellationToken cancellationToken) => Results.Ok())
            .WithDescription("Редактирование продажи")
            .WithDisplayName("Редактирование продажи")
            .RequireAnyPermission(PermissionCodes.SALES_EDIT);

        sales.MapGet("/{id}/content", (
                ISender sender,
                string id,
                CancellationToken cancellationToken) => Results.Ok())
            .WithDescription("Получение содержания продажи")
            .WithDisplayName("Получение содержания продажи")
            .RequireAnyPermission(PermissionCodes.SALES_GET);

        sales.MapGet("/", (
                ISender sender,
                [AsParameters] GetSalesRequest request,
                CancellationToken token) => Results.Ok())
            .WithDescription("Получение списка продаж")
            .WithDisplayName("Получение продаж")
            .RequireAnyPermission(PermissionCodes.SALES_GET);
    }
}
