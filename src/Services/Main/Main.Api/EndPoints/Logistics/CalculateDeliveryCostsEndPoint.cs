using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Logistics;
using Main.Application.Handlers.Logistics.CalculateDeliveryCost;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Logistics;

public record CalculateDeliveryCostRequest(
    string StorageFrom,
    string StorageTo,
    int CurrencyId,
    LogisticsCalculationMode Mode,
    IEnumerable<LogisticsItemDto> Items);

public record CalculateDeliveryCostResponse(DeliveryCostDto DeliveryCost);

public class CalculateDeliveryCostsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/logistics/calculate", async (ISender sender, CalculateDeliveryCostRequest request, CancellationToken token) =>
        {
            var query = new CalculateDeliveryCostQuery(request.StorageFrom, request.StorageTo, request.CurrencyId, 
                request.Items, request.Mode);
            var result = await sender.Send(query, token);
            return Results.Ok(new CalculateDeliveryCostResponse(result.DeliveryCost));
        }).WithTags("Logistics")
        .WithDescription("Расчет стоимости доставки между складами")
        .WithDisplayName("Расчет стоимости доставки между складами")
        .RequireAnyPermission(PermissionCodes.LOGISTICS_CALCULATE);
    }
}