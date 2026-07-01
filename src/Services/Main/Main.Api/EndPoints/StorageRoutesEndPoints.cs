using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.StorageRoutes.AddStorageRoute;
using Main.Application.Handlers.StorageRoutes.DeleteStorageRoute;
using Main.Application.Handlers.StorageRoutes.EditStorageRoute;
using Main.Application.Handlers.StorageRoutes.GetStorageRouteById;
using Main.Application.Handlers.StorageRoutes.GetStorageRoutes;
using Main.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints;

public record AddStorageRouteRequest(
    string StorageFrom,
    string StorageTo,
    int Distance,
    RouteType RouteType,
    LogisticPricingType PricingType,
    int DeliveryTime,
    decimal PriceKg,
    decimal PriceM3,
    int CurrencyId,
    decimal PricePerOrder,
    decimal? MinimumPrice,
    Guid? CarrierId);

public record AddStorageRouteResponse(Guid RouteId);

public record EditStorageRouteRequest(PatchStorageRouteDto PatchStorageRoute);

public record GetStorageRouteResponse(StorageRouteDto StorageRoute);

public record GetStorageRoutesRequest(
    [FromQuery(Name = "from")] string? StorageFrom,
    [FromQuery(Name = "to")] string? StorageTo,
    [FromQuery(Name = "isActive")] bool? IsActive,
    [FromQuery(Name = "page")] int Page,
    [FromQuery(Name = "limit")] int Limit);

public record GetStorageRoutesResponse(List<StorageRouteDto> StorageRoutes);

public class StorageRoutesEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routes = app.MapGroup("/storages/routes")
            .WithTags("Storage Routes");

        routes.MapPost("", async (ISender sender, AddStorageRouteRequest request, CancellationToken token) =>
            {
                var result = await sender.Send(
                    new AddStorageRouteCommand(
                        request.StorageFrom,
                        request.StorageTo,
                        request.Distance,
                        request.RouteType,
                        request.PricingType,
                        request.DeliveryTime,
                        request.PriceKg,
                        request.PriceM3,
                        request.CurrencyId,
                        request.PricePerOrder,
                        request.MinimumPrice ?? 0,
                        request.CarrierId), 
                    token);
                return Results.Created($"/storages/routes/{result.RouteId}", new AddStorageRouteResponse(result.RouteId));
            })
            .WithName("CreateStorageRoute")
            .WithSummary("Создать маршрут склада")
            .WithDescription("Создание маршрута")
            .WithDisplayName("Создание маршрута")
            .Accepts<AddStorageRouteRequest>(false, "application/json")
            .Produces<AddStorageRouteResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.STORAGE_ROUTES_CREATE);

        routes.MapDelete("/{id:guid}", async (ISender sender, Guid id, CancellationToken token) =>
            {
                await sender.Send(new DeleteStorageRouteCommand(id), token);
                return Results.NoContent();
            })
            .WithName("DeleteStorageRoute")
            .WithSummary("Удалить маршрут склада")
            .WithDescription("Удаление маршрута")
            .WithDisplayName("Удаление маршрута")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.STORAGE_ROUTES_DELETE);

        routes.MapPatch("/{id:guid}", async (
                ISender sender,
                Guid id,
                EditStorageRouteRequest request,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new EditStorageRouteCommand(id, request.PatchStorageRoute), cancellationToken);
                return Results.Ok();
            })
            .WithName("EditStorageRoute")
            .WithSummary("Редактировать маршрут склада")
            .WithDescription("Обновление маршрута")
            .WithDisplayName("Обновление маршрута")
            .Accepts<EditStorageRouteRequest>(false, "application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.STORAGE_ROUTES_EDIT);

        routes.MapGet("/{id:guid}", async (ISender sender, Guid id, CancellationToken token) =>
            {
                var result = await sender.Send(new GetStorageRouteByIdQuery(id), token);
                return Results.Ok(new GetStorageRouteResponse(result.StorageRoute));
            })
            .WithName("GetStorageRoute")
            .WithSummary("Получить маршрут склада")
            .WithDescription("Получение маршрута")
            .WithDisplayName("Получение маршрута")
            .Produces<GetStorageRouteResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.STORAGE_ROUTES_GET);

        routes.MapGet("", async (
                ISender sender,
                [AsParameters] GetStorageRoutesRequest request,
                CancellationToken token) =>
            {
                var query = new GetStorageRoutesQuery(
                    request.StorageFrom,
                    request.StorageTo,
                    request.IsActive,
                    new Pagination(request.Page, request.Limit));
                var result = await sender.Send(query, token);
                return Results.Ok(new GetStorageRoutesResponse(result.StorageRoutes));
            })
            .WithName("GetStorageRoutes")
            .WithSummary("Получить маршруты складов")
            .WithDescription("Получение маршрутов")
            .WithDisplayName("Получение маршрутов")
            .Produces<GetStorageRoutesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.STORAGE_ROUTES_GET);
    }
}
