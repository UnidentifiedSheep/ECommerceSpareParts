using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Logistics;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.Logistics.CalculateDeliveryCost;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints;

public record CalculateDeliveryCostRequest
{
    [JsonPropertyName("storageFrom")]
    public required string StorageFrom { get; init; }

    [JsonPropertyName("storageTo")]
    public required string StorageTo { get; init; }

    [JsonPropertyName("mode")]
    public required LogisticsCalculationMode Mode { get; init; }

    [JsonPropertyName("items")]
    public required IEnumerable<LogisticsItemDto> Items { get; init; }
}

public record CalculateDeliveryCostResponse(DeliveryCostDto DeliveryCost, StorageRouteDto UsedRoute);

public class CalculateDeliveryCostsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/logistics/calculate",
                async (
                    ISender sender,
                    CalculateDeliveryCostRequest request,
                    CancellationToken token) =>
                {
                    var query = new CalculateDeliveryCostQuery(
                        request.StorageFrom,
                        request.StorageTo,
                        request.Items,
                        request.Mode);
                    var result = await sender.Send(query, token);
                    return Results.Ok(new CalculateDeliveryCostResponse(result.DeliveryCost, result.Route));
                })
            .WithTags("Logistics")
            .WithName("CalculateDeliveryCost")
            .WithSummary("Рассчитать стоимость доставки")
            .WithDescription("Расчет стоимости доставки между складами")
            .WithDisplayName("Расчет стоимости доставки между складами")
            .Accepts<CalculateDeliveryCostRequest>(false, "application/json")
            .Produces<CalculateDeliveryCostResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.LOGISTICS_CALCULATE);
    }
}