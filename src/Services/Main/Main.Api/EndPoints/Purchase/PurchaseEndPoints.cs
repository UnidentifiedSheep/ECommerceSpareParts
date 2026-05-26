using System.Text.Json.Serialization;
using Abstractions.Interfaces;
using Abstractions.Models;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Purchase;
using Main.Application.Handlers.Purchases.CreatePurchase;
using Main.Application.Handlers.Purchases.DeleteFullPurchase;
using Main.Application.Handlers.Purchases.EditPurchase;
using Main.Application.Handlers.Purchases.GetPurchase;
using Main.Application.Handlers.Purchases.GetPurchaseContent;
using Main.Application.Handlers.Purchases.GetPurchaseLogistic;
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

public record EditPurchaseRequest(
    IEnumerable<EditPurchaseDto> Content,
    int CurrencyId,
    string? Comment,
    DateTime PurchaseDateTime,
    bool WithLogistics,
    string? StorageFrom);

public record GetPurchaseContentResponse(IEnumerable<PurchaseContentDto> Content);

public record GetPurchaseLogisticResponse(PurchaseLogisticDto PurchaseLogistic);

public record GetPurchasesResponse(IEnumerable<PurchaseDto> Purchases);

public record GetPurchasesRequest : SortablePaginationQueryModel
{
    [FromQuery(Name = "rangeStartDate")] public DateTime RangeStartDate { get; init; }
    [FromQuery(Name = "rangeEndDate")] public DateTime RangeEndDate { get; init; }
    [FromQuery(Name = "supplierId")] public Guid? SupplierId { get; init; }
    [FromQuery(Name = "currencyId")] public int? CurrencyId { get; init; }
    [FromQuery(Name = "searchTerm")] public string? SearchTerm { get; init; }
}

public class PurchaseEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var purchases = app.MapGroup("/purchases")
            .WithTags("Purchases");

        purchases.MapPost("/", async (
                IUserContext user,
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
                await sender.Send(command, token);
                return Results.Ok();
            })
            .WithName("CreatePurchase")
            .WithSummary("Создать закупку")
            .WithDescription("Создание новой закупки")
            .WithDisplayName("Создание новой закупки")
            .Accepts<CreatePurchaseRequest>(false, "application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.PURCHASE_CREATE);

        purchases.MapDelete("/{purchaseId}", async (
                ISender sender,
                string purchaseId,
                IUserContext user,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteFullPurchaseCommand(purchaseId, user.UserId), cancellationToken);
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

        purchases.MapGet("/{id}/contents", async (ISender sender, string id, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetPurchaseContentQuery(id), ct);
                return Results.Ok(result.Adapt<GetPurchaseContentResponse>());
            })
            .WithName("GetPurchaseContent")
            .WithSummary("Получить содержимое закупки")
            .WithDescription("Получение содержания закупки")
            .WithDisplayName("Получение содержания закупки")
            .Produces<GetPurchaseContentResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PURCHASE_GET);

        purchases.MapGet("/{id}/logistics", async (ISender sender, string id, CancellationToken token) =>
            {
                var result = await sender.Send(new GetPurchaseLogisticQuery(id), token);
                return Results.Ok(new GetPurchaseLogisticResponse(result.PurchaseLogistic));
            })
            .WithName("GetPurchaseLogistics")
            .WithSummary("Получить логистику закупки")
            .WithDescription("Получение логистики закупки")
            .WithDisplayName("Получение логистики закупки")
            .Produces<GetPurchaseLogisticResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PURCHASE_GET);

        purchases.MapGet("/", async (
                ISender sender,
                [AsParameters] GetPurchasesRequest request,
                CancellationToken token) =>
            {
                var query = new GetPurchasesQuery(
                    request.RangeStartDate,
                    request.RangeEndDate,
                    request,
                    request.SupplierId,
                    request.CurrencyId,
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
