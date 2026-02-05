using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Logistics;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Application.Handlers.Logistics.CalculateDeliveryCost;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Logistics;

public record CalculateDeliveryCostRequest(
    string StorageFrom,
    string StorageTo,
    LogisticsCalculationMode Mode,
    IEnumerable<LogisticsItemDto> Items);

public record CalculateDeliveryCostResponse(DeliveryCostDto DeliveryCost, StorageRouteDto UsedRoute);

public class CalculateDeliveryCostsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/logistics/calculate", async (ISender sender, CalculateDeliveryCostRequest request, CancellationToken token) =>
        {
            var query = new CalculateDeliveryCostQuery(request.StorageFrom, request.StorageTo, request.Items, request.Mode);
            var result = await sender.Send(query, token);
            return Results.Ok(new CalculateDeliveryCostResponse(result.DeliveryCost, result.Route));
        }).WithTags("Logistics")
        .WithDescription("Расчет стоимости доставки между складами")
        .WithDisplayName("Расчет стоимости доставки между складами")
        .RequireAnyPermission(PermissionCodes.LOGISTICS_CALCULATE);
    }
}