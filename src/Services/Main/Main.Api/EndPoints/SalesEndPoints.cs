using System.Text.Json.Serialization;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Sales.CreateSale;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints;

public record CreateSaleRequest
{
    [JsonPropertyName("buyerId")]
    public required Guid  BuyerId { get; init; }
    
    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }
    
    [JsonPropertyName("storageName")]
    public required string StorageName { get; init; }
    
    [JsonPropertyName("saleDateTime")]
    public required DateTime SaleDateTime { get; init; }
    
    [JsonPropertyName("contents")]
    public required IEnumerable<NewSaleContentDto> Contents { get; init; }
    
    [JsonPropertyName("comment")]
    public string? Comment { get; init; }

    [JsonPropertyName("payedSum")]
    public decimal? PayedSum { get; init; }
    
    [JsonPropertyName("confirmationCode")]
    public string? ConfirmationCode { get; init; }
}

public record CreateSaleResponse
{
    [JsonPropertyName("sale")]
    public required SaleDto Sale { get; init; }
}

public record EditSaleRequest(
    IEnumerable<EditSaleContentDto> EditedContent,
    int CurrencyId,
    DateTime SaleDateTime,
    string? Comment,
    bool SellFromOtherStorages);

public record GetSaleContentResponse(IEnumerable<SaleContentDto> Content);

public record GetSalesResponse(IEnumerable<SaleDto> Sales);

public record GetSalesRequest : PaginationQueryModel
{
    [FromQuery(Name = "rangeStartDate")] 
    public DateTime RangeStartDate { get; init; }
    
    [FromQuery(Name = "rangeEndDate")] 
    public DateTime RangeEndDate { get; init; }
    
    [FromQuery(Name = "buyerId")] 
    public Guid? BuyerId { get; init; }
    
    [FromQuery(Name = "currencyId")] 
    public int? CurrencyId { get; init; }
    
    [FromQuery(Name = "sortBy")] 
    public string? SortBy { get; init; }
    
    [FromQuery(Name = "searchTerm")] 
    public string? SearchTerm { get; init; }
}

public class SalesEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var sales = app.MapGroup("/sales")
            .WithTags("Sales");

        sales.MapPost("/", async (
                ISender sender,
                CreateSaleRequest request,
                CancellationToken token) =>
            {
                var result = await sender.Send(new CreateSaleCommand(
                    request.BuyerId,
                    request.CurrencyId,
                    request.StorageName,
                    request.SaleDateTime,
                    request.Contents,
                    request.Comment,
                    request.PayedSum,
                    request.ConfirmationCode), token);
                
                return Results.Ok(new CreateSaleResponse
                {
                    Sale = result.Sale
                });
            })
            .WithDescription("Создание новой продажи")
            .WithName("CreateSale")
            .WithSummary("Создать продажу")
            .WithDisplayName("Создание новой продажи")
            .Accepts<CreateSaleRequest>(false, "application/json")
            .Produces<CreateSaleResponse>()
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
