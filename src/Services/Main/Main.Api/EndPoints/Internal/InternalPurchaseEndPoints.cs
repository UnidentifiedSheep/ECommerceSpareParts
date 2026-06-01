using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Purchase;
using Main.Application.Handlers.Purchases.GetFullPurchase;
using MediatR;

namespace Main.Api.EndPoints.Internal;

public record InternalGetFullPurchaseResponse
{
    [JsonPropertyName("purchase")]
    public required PurchaseDto Purchase { get; init; }
    
    [JsonPropertyName("contents")]
    public required IEnumerable<PurchaseContentDto> Contents { get; init; }
}

public class InternalPurchaseEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/internal/purchases")
            .WithGroupName("Internal Purchase")
            .WithTags("InternalPurchase");

        group.MapGet("{id:guid}", async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetFullPurchaseQuery(id), cancellationToken);
                return Results.Ok(new InternalGetFullPurchaseResponse
                {
                    Purchase = result.Purchase,
                    Contents = result.Contents
                });
            })
            .RequireAllPermissions(PermissionCodes.PURCHASE_GET)
            .WithName("InternalFullPurchase")
            .WithDisplayName("Internal service full purchase")
            .WithSummary("Получить полную закупку для внутреннего сервиса")
            .WithDescription("Получение закупки с ее содержимым для внутренних интеграций")
            .Produces<InternalGetFullPurchaseResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
