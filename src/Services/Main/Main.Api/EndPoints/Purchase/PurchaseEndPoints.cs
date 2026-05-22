using Abstractions.Interfaces;
using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Amw.Purchase;
using Main.Application.Handlers.Purchases.CreatePurchase;
using Main.Application.Handlers.Purchases.DeleteFullPurchase;
using Main.Application.Handlers.Purchases.EditFullPurchase;
using Main.Application.Handlers.Purchases.GetPurchase;
using Main.Application.Handlers.Purchases.GetPurchaseContent;
using Main.Application.Handlers.Purchases.GetPurchaseLogistic;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Purchase;

public record CreatePurchaseRequest(
    Guid SupplierId,
    int CurrencyId,
    string StorageName,
    DateTime PurchaseDate,
    IEnumerable<NewPurchaseContentDto> PurchaseContent,
    string? Comment,
    decimal? PayedSum,
    bool WithLogistics,
    string? StorageFrom);

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

public class GetPurchasesRequest
{
    [FromQuery(Name = "rangeStartDate")] public DateTime RangeStartDate { get; set; }
    [FromQuery(Name = "rangeEndDate")] public DateTime RangeEndDate { get; set; }
    [FromQuery(Name = "page")] public int Page { get; set; }
    [FromQuery(Name = "limit")] public int Limit { get; set; }
    [FromQuery(Name = "supplierId")] public Guid? SupplierId { get; set; }
    [FromQuery(Name = "currencyId")] public int? CurrencyId { get; set; }
    [FromQuery(Name = "sortBy")] public string? SortBy { get; set; }
    [FromQuery(Name = "searchTerm")] public string? SearchTerm { get; set; }
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
                    user.UserId,
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
            .WithDescription("Создание новой закупки")
            .WithDisplayName("Создание новой закупки")
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
            .WithDescription("Удаление закупки")
            .WithDisplayName("Удаление закупки")
            .RequireAnyPermission(PermissionCodes.PURCHASE_DELETE);

        purchases.MapPut("/{purchaseId}", async (
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
            .WithDescription("Редактирование существующей закупки")
            .WithDisplayName("Редактирование закупки")
            .RequireAnyPermission(PermissionCodes.PURCHASE_EDIT);

        purchases.MapGet("/{id}/contents", async (ISender sender, string id, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetPurchaseContentQuery(id), ct);
                return Results.Ok(result.Adapt<GetPurchaseContentResponse>());
            })
            .WithDescription("Получение содержания закупки")
            .WithDisplayName("Получение содержания закупки")
            .RequireAnyPermission(PermissionCodes.PURCHASE_GET);

        purchases.MapGet("/{id}/logistics", async (ISender sender, string id, CancellationToken token) =>
            {
                var result = await sender.Send(new GetPurchaseLogisticQuery(id), token);
                return Results.Ok(new GetPurchaseLogisticResponse(result.PurchaseLogistic));
            })
            .WithDescription("Получение логистики закупки")
            .WithDisplayName("Получение логистики закупки")
            .RequireAnyPermission(PermissionCodes.PURCHASE_GET);

        purchases.MapGet("/", async (
                ISender sender,
                [AsParameters] GetPurchasesRequest request,
                CancellationToken token) =>
            {
                var query = new GetPurchasesQuery(
                    request.RangeStartDate,
                    request.RangeEndDate,
                    new Pagination(request.Page, request.Limit),
                    request.SupplierId,
                    request.CurrencyId,
                    request.SortBy,
                    request.SearchTerm);
                var result = await sender.Send(query, token);
                return Results.Ok(result.Adapt<GetPurchasesResponse>());
            })
            .WithDescription("Получение списка покупок")
            .WithDisplayName("Получение покупок")
            .RequireAnyPermission(PermissionCodes.PURCHASE_GET);
    }
}
