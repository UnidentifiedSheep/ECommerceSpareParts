using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Sale;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints;

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
            .WithName("CreateSale")
            .WithSummary("Создать продажу")
            .WithDisplayName("Создание новой продажи")
            .Accepts<CreateSaleRequest>(false, "application/json")
            .Produces(200)
            .Produces(401)
            .RequireAnyPermission(PermissionCodes.SALES_CREATE);

        sales.MapDelete("/{saleId}", (
                ISender sender,
                IUserContext user,
                string saleId,
                CancellationToken token) => Results.NoContent())
            .WithDescription("Удаление продажи")
            .WithName("DeleteSale")
            .WithSummary("Удалить продажу")
            .WithDisplayName("Удаление продажи")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.SALES_DELETE);

        sales.MapPut("/{saleId}", (
                ISender sender,
                string saleId,
                EditSaleRequest request,
                IUserContext user,
                CancellationToken cancellationToken) => Results.Ok())
            .WithDescription("Редактирование продажи")
            .WithName("EditSale")
            .WithSummary("Редактировать продажу")
            .WithDisplayName("Редактирование продажи")
            .Accepts<EditSaleRequest>(false, "application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.SALES_EDIT);

        sales.MapGet("/{id}/content", (
                ISender sender,
                string id,
                CancellationToken cancellationToken) => Results.Ok())
            .WithDescription("Получение содержания продажи")
            .WithName("GetSaleContent")
            .WithSummary("Получить содержимое продажи")
            .WithDisplayName("Получение содержания продажи")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.SALES_GET);

        sales.MapGet("/", (
                ISender sender,
                [AsParameters] GetSalesRequest request,
                CancellationToken token) => Results.Ok())
            .WithDescription("Получение списка продаж")
            .WithName("GetSales")
            .WithSummary("Получить продажи")
            .WithDisplayName("Получение продаж")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.SALES_GET);
    }
}
