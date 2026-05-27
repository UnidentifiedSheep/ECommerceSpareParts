using System.Text.Json.Serialization;
using Abstractions.Interfaces;
using Abstractions.Models;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Purchase;
using Main.Application.Handlers.Purchases.CreatePurchase;
using Main.Application.Handlers.Purchases.DeletePurchase;
using Main.Application.Handlers.Purchases.EditPurchase;
using Main.Application.Handlers.Purchases.GetPurchase;
using Main.Application.Handlers.Purchases.GetPurchaseContent;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Purchase;

public record CreatePurchaseRequest
{
    [JsonPropertyName("supplierId")]
    public required Guid SupplierId { get; init; }
    
    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }
    
    [JsonPropertyName("storageName")]
    public required string StorageName { get; init; }
    
    [JsonPropertyName("purchaseDate")]
    public required DateTime PurchaseDate { get; init; }
    
    [JsonPropertyName("purchaseContent")]
    public required IEnumerable<NewPurchaseContentDto> PurchaseContent { get; init; }
    
    [JsonPropertyName("withLogistics")]
    public required bool WithLogistics { get; init; }
    
    [JsonPropertyName("comment")]
    public string? Comment { get; init; }
    
    [JsonPropertyName("payedSum")]
    public decimal? PayedSum { get; init; }
    
    [JsonPropertyName("storageFrom")]
    public string? StorageFrom { get; init; }
}

public record CreatePurchaseResponse
{
    [JsonPropertyName("purchase")]
    public required PurchaseDto Purchase { get; init; }
}

public record EditPurchaseRequest(
    IEnumerable<EditPurchaseDto> Content,
    int CurrencyId,
    string? Comment,
    DateTime PurchaseDateTime,
    bool WithLogistics,
    string? StorageFrom);

public record GetPurchaseContentResponse
{
    public required IReadOnlyList<PurchaseContentDto> Content { get; init; }
}

public record GetPurchaseLogisticResponse(PurchaseLogisticDto PurchaseLogistic);

public record GetPurchasesResponse(IEnumerable<PurchaseDto> Purchases);

public record GetPurchasesRequest : SortablePaginationQueryModel
{
    [FromQuery(Name = "rangeStartDate")] public DateTime RangeStartDate { get; init; }
    [FromQuery(Name = "rangeEndDate")] public DateTime RangeEndDate { get; init; }
    [FromQuery(Name = "supplierIds")] public Guid[] SupplierIds { get; init; } = [];
    [FromQuery(Name = "currencyIds")] public int[] CurrencyIds { get; init; } = [];
    [FromQuery(Name = "productIds")] public int[] ProductIds { get; init; } = [];
    [FromQuery(Name = "searchTerm")] public string? SearchTerm { get; init; }
}

public class PurchaseEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var purchases = app.MapGroup("/purchases")
            .WithTags("Purchases");

        purchases.MapPost("/", async (
                ISender sender,
                CreatePurchaseRequest request,
                CancellationToken token) =>
            {
                var command = new CreatePurchaseCommand(
                    request.SupplierId,
                    request.CurrencyId,
                    request.StorageName,
                    request.PurchaseDate,
                    request.PurchaseContent,
                    request.Comment,
                    request.PayedSum,
                    request.WithLogistics,
                    request.StorageFrom);
                var result = await sender.Send(command, token);
                return Results.Created(
                    new Uri($"/purchases/{result.Purchase.Id}"),
                    new CreatePurchaseResponse
                    {
                        Purchase = result.Purchase 
                    });
            })
            .WithName("CreatePurchase")
            .WithSummary("Создать закупку")
            .WithDescription("Создание новой закупки")
            .WithDisplayName("Создание новой закупки")
            .Accepts<CreatePurchaseRequest>(false, "application/json")
            .Produces<CreatePurchaseResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.PURCHASE_CREATE);

        purchases.MapDelete("/{purchaseId}", async (
                ISender sender,
                Guid purchaseId,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeletePurchaseCommand(purchaseId), cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeletePurchase")
            .WithSummary("Удалить закупку")
            .WithDescription("Удаление закупки")
            .WithDisplayName("Удаление закупки")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PURCHASE_DELETE);

        purchases.MapPut("/{purchaseId:guid}", async (
                ISender sender,
                Guid purchaseId,
                EditPurchaseRequest request,
                CancellationToken cancellationToken,
                IUserContext user) =>
            {
                var command = new EditPurchaseCommand(
                    request.Content,
                    purchaseId,
                    request.CurrencyId,
                    request.Comment,
                    request.PurchaseDateTime,
                    user.UserId,
                    request.WithLogistics,
                    request.StorageFrom);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .WithName("EditPurchase")
            .WithSummary("Редактировать закупку")
            .WithDescription("Редактирование существующей закупки")
            .WithDisplayName("Редактирование закупки")
            .Accepts<EditPurchaseRequest>(false, "application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PURCHASE_EDIT);

        purchases.MapGet("/{id:guid}/contents", async (ISender sender, Guid id, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetPurchaseContentQuery(id), ct);
                return Results.Ok(new GetPurchaseContentResponse
                {
                    Content = result.Content
                });
            })
            .WithName("GetPurchaseContent")
            .WithSummary("Получить содержимое закупки")
            .WithDescription("Получение содержания закупки")
            .WithDisplayName("Получение содержания закупки")
            .Produces<GetPurchaseContentResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PURCHASE_GET);

        purchases.MapGet("/", async (
                ISender sender,
                [AsParameters] GetPurchasesRequest request,
                CancellationToken token) =>
            {
                var query = new GetPurchasesQuery(
                    new RangeModel<DateTime>(request.RangeStartDate, request.RangeEndDate),
                    request,
                    request.SupplierIds,
                    request.CurrencyIds,
                    request.ProductIds,
                    request.SortBy,
                    request.SearchTerm);
                var result = await sender.Send(query, token);
                return Results.Ok(result.Adapt<GetPurchasesResponse>());
            })
            .WithName("GetPurchases")
            .WithSummary("Получить закупки")
            .WithDescription("Получение списка покупок")
            .WithDisplayName("Получение покупок")
            .Produces<GetPurchasesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.PURCHASE_GET);
    }
}
