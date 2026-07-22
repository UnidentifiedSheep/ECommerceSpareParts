using System.Text.Json.Serialization;
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
using Main.Application.Handlers.Purchases.GetPurchases;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints;

public record CreatePurchaseRequest
{
    [JsonPropertyName("supplierUserId")]
    public required Guid SupplierUserId { get; init; }

    [JsonPropertyName("supplierOrganizationId")]
    public required Guid SupplierOrganizationId { get; init; }

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
    string? StorageFrom
);

public record GetPurchaseContentResponse
{
    public required IReadOnlyList<PurchaseContentDto> Content { get; init; }
}

public record GetPurchaseLogisticResponse(PurchaseLogisticDto PurchaseLogistic);

public record GetPurchaseResponse(PurchaseDto Purchase);

public record GetPurchasesResponse(IEnumerable<PurchaseDto> Purchases);

public record GetPurchasesRequest : SortablePaginationQueryModel
{
    [FromQuery(Name = "rangeStartDate")]
    public DateTime RangeStartDate { get; init; }

    [FromQuery(Name = "rangeEndDate")]
    public DateTime RangeEndDate { get; init; }

    [FromQuery(Name = "supplierOrganizationIds")]
    public Guid[] SupplierOrganizationIds { get; init; } = [];

    [FromQuery(Name = "currencyIds")]
    public int[] CurrencyIds { get; init; } = [];

    [FromQuery(Name = "productIds")]
    public int[] ProductIds { get; init; } = [];

    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }
}

public class PurchaseEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var purchases = app.MapGroup("/purchases")
            .WithTags("Purchases");

        purchases.MapPost(
                "/",
                async (
                    ISender sender,
                    CreatePurchaseRequest request,
                    CancellationToken token) =>
                {
                    var command = new CreatePurchaseCommand(
                        request.SupplierUserId,
                        request.SupplierOrganizationId,
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

        purchases.MapDelete(
                "/{purchaseId}",
                async (
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

        purchases.MapPut(
                "/{purchaseId:guid}",
                async (
                    ISender sender,
                    Guid purchaseId,
                    EditPurchaseRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var command = new EditPurchaseCommand(
                        request.Content,
                        purchaseId,
                        request.CurrencyId,
                        request.Comment,
                        request.PurchaseDateTime,
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

        purchases.MapGet(
                "/{purchaseId:guid}",
                async (
                    ISender sender,
                    Guid purchaseId,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new GetPurchaseQuery(purchaseId, null),
                        cancellationToken);

                    return Results.Ok(new GetPurchaseResponse(result.Purchase));
                })
            .WithName("GetPurchase")
            .WithSummary("Получить закупку")
            .WithDescription("Получение закупки по идентификатору")
            .WithDisplayName("Получение закупки")
            .Produces<GetPurchaseResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PURCHASE_GET);

        purchases.MapGet(
                "/{id:guid}/contents",
                async (
                    ISender sender,
                    Guid id,
                    CancellationToken ct) =>
                {
                    var result = await sender.Send(new GetPurchaseContentQuery(id), ct);
                    return Results.Ok(
                        new GetPurchaseContentResponse
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

        purchases.MapGet(
                "/",
                async (
                    ISender sender,
                    [AsParameters] GetPurchasesRequest request,
                    CancellationToken token) =>
                {
                    var query = new GetPurchasesQuery(
                        new RangeModel<DateTime>(request.RangeStartDate, request.RangeEndDate),
                        request,
                        request.SupplierOrganizationIds,
                        request.CurrencyIds,
                        request.ProductIds,
                        request.SortBy,
                        request.SearchTerm);
                    var result = await sender.Send(query, token);
                    return Results.Ok(new GetPurchasesResponse(result.Purchases));
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
