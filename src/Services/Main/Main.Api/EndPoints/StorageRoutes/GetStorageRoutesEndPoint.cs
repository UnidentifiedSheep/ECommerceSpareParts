using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Application.Handlers.StorageRoutes.GetStorageRoutes;
using Main.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.StorageRoutes;

public record GetStorageRoutesRequest(
    [FromQuery(Name = "from")] string? StorageFrom,
    [FromQuery(Name = "to")] string? StorageTo,
    [FromQuery(Name = "isActive")] bool? IsActive,
    [FromQuery(Name = "page")] int Page,
    [FromQuery(Name = "limit")] int Limit);
public record GetStorageRoutesResponse(List<StorageRouteDto> StorageRoutes);

public class GetStorageRoutesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/storages/routes", 
            async (ISender sender, [AsParameters] GetStorageRoutesRequest request, CancellationToken token) =>
        {
            var query = new GetStorageRoutesQuery(request.StorageFrom, request.StorageTo, request.IsActive, 
                new PaginationModel(request.Page, request.Limit));
            var result = await sender.Send(query, token);
            return Results.Ok(new GetStorageRoutesResponse(result.StorageRoutes));
        }).WithTags("Storage Routes")
            .WithDescription("Получение маршрутов")
            .WithDisplayName("Получение маршрутов")
            .RequireAnyPermission(PermissionCodes.STORAGE_ROUTES_GET);
    }
}