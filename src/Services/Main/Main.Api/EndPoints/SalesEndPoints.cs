using System.Text.Json.Serialization;
using Abstractions.Interfaces;
using Abstractions.Models;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Sales;
using Main.Application.Handlers.Sales.CreateSale;
using Main.Application.Handlers.Sales.EditSale;
using Main.Application.Handlers.Sales.GetSale;
using Main.Application.Handlers.Sales.GetSales;
using Main.Enums;
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

    [JsonPropertyName("forcePayment")]
    public bool ForcePayment { get; init; }
}

public record CreateSaleResponse
{
    [JsonPropertyName("sale")]
    public required SaleDto Sale { get; init; }
}

public record GetSalesRequest : SortablePaginationQueryModel
{
    [FromQuery(Name = "rangeStartDate")] 
    public DateTime RangeStartDate { get; init; }
    
    [FromQuery(Name = "rangeEndDate")] 
    public DateTime RangeEndDate { get; init; }
    
    [FromQuery(Name = "buyerIds")] 
    public Guid[] BuyerIds { get; init; } = [];
    
    [FromQuery(Name = "currencyIds")] 
    public int[] CurrencyIds { get; init; } = [];

    [FromQuery(Name = "productIds")] 
    public int[] ProductIds { get; init; } = [];
    
    [FromQuery(Name = "searchTerm")] 
    public string? SearchTerm { get; init; }
    
    [FromQuery(Name = "state")]
    public SaleState[] States { get; init; } = [];
}
public record GetSalesResponse
{
    [JsonPropertyName("sales")]
    public required IReadOnlyList<SaleDto> Sales { get; init; }
}

public record GetSaleResponse
{
    [JsonPropertyName("sale")]
    public required SaleDto Sale { get; init; }
}

public record EditSaleRequest
{
    [JsonPropertyName("content")]
    public required IEnumerable<EditSaleContentDto> Content { get; init; }

    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }

    [JsonPropertyName("saleDateTime")]
    public required DateTime SaleDateTime { get; init; }

    [JsonPropertyName("comment")]
    public string? Comment { get; init; }

    [JsonPropertyName("confirmationCode")]
    public string? ConfirmationCode { get; init; }

    [JsonPropertyName("forcePayment")]
    public bool ForcePayment { get; init; }
}

public record GetSaleContentResponse(IReadOnlyList<SaleContentDto> Content);

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
                    request.ConfirmationCode,
                    request.ForcePayment), token);
                
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

        sales.MapDelete("/{saleId:guid}", async (
                ISender sender,
                [FromHeader(Name = "If-Match")] uint rowVersion,
                Guid saleId,
                CancellationToken token) =>
            {
                await sender.Send(new DeleteSaleCommand(saleId, rowVersion), token);
                return Results.NoContent();
            })
            .WithDescription("Удаление продажи")
            .WithName("DeleteSale")
            .WithSummary("Удалить продажу")
            .WithDisplayName("Удаление продажи")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAnyPermission(PermissionCodes.SALES_DELETE);

        sales.MapGet("/{saleId:guid}", async (
                ISender sender,
                Guid saleId,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetSaleQuery(saleId, null),
                    cancellationToken);

                return Results.Ok(new GetSaleResponse
                {
                    Sale = result.Sale
                });
            })
            .WithDescription("Получение продажи по идентификатору")
            .WithName("GetSale")
            .WithSummary("Получить продажу")
            .WithDisplayName("Получение продажи")
            .Produces<GetSaleResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.SALES_GET);

        sales.MapPut("/{saleId:guid}", async (
                ISender sender,
                [FromHeader(Name = "If-Match")] uint rowVersion,
                Guid saleId,
                EditSaleRequest request,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(
                    new EditSaleCommand(
                        saleId,
                        rowVersion,
                        request.Content,
                        request.CurrencyId,
                        request.SaleDateTime,
                        request.Comment,
                        request.ConfirmationCode,
                        request.ForcePayment),
                    cancellationToken);

                return Results.NoContent();
            })
            .WithDescription("Редактирование продажи")
            .WithName("EditSale")
            .WithSummary("Редактировать продажу")
            .WithDisplayName("Редактирование продажи")
            .Accepts<EditSaleRequest>(false, "application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAnyPermission(PermissionCodes.SALES_EDIT);

        sales.MapGet("/{id:guid}/contents", async (
                ISender sender,
                Guid id,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetSaleContentQuery(id), cancellationToken);
                return Results.Ok(new GetSaleContentResponse(result.Content));
            })
            .WithDescription("Получение содержания продажи")
            .WithName("GetSaleContent")
            .WithSummary("Получить содержимое продажи")
            .WithDisplayName("Получение содержания продажи")
            .Produces<GetSaleContentResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.SALES_GET);

        sales.MapGet("/", async (
                ISender sender,
                [AsParameters] GetSalesRequest request,
                CancellationToken token) =>
            {
                var result = await sender.Send(new GetSalesQuery(
                    new RangeModel<DateTime>(request.RangeStartDate, request.RangeEndDate),
                    request,
                    request.BuyerIds,
                    request.CurrencyIds,
                    request.ProductIds,
                    request.States,
                    request.SortBy,
                    request.SearchTerm), token);

                return Results.Ok(new GetSalesResponse
                {
                    Sales = result.Sales
                });
            })
            .WithDescription("Получение списка продаж")
            .WithName("GetSales")
            .WithSummary("Получить продажи")
            .WithDisplayName("Получение продаж")
            .Produces<GetSalesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.SALES_GET);
    }
}
