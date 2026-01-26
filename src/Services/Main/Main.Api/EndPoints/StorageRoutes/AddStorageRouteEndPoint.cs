using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.StorageRoutes.AddStorageRoute;
using Main.Enums;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.StorageRoutes;

public record AddStorageRouteRequest(string StorageFrom, string StorageTo, int Distance, RouteType RouteType, 
    LogisticPricingType PricingType, int DeliveryTime, decimal PriceKg, decimal PriceM3, int CurrencyId,
    decimal PricePerOrder);
public record AddStorageRouteResponse(Guid RouteId);

public class AddStorageRouteEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/storages/routes", 
                async (ISender sender, AddStorageRouteRequest request, CancellationToken token) =>
                {
                    var command = request.Adapt<AddStorageRouteCommand>();
                    var result =  await sender.Send(command, token);
                    return Results.Created($"/storages/routes/{result.RouteId}", new AddStorageRouteResponse(result.RouteId));
                })
            .WithTags("Storage Routes")
            .WithDescription("Создание маршрута")
            .WithDisplayName("Создание маршрута")
            .RequireAnyPermission(PermissionCodes.STORAGE_ROUTES_CREATE);
    }
}