using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Application.Handlers.Purchases.GetPurchaseLogistic;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Purchase;

public record GetPurchaseLogisticResponse(PurchaseLogisticDto PurchaseLogistic);

public class GetPurchaseLogisticEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/purchases/{id}/logistics", async (ISender sender, string id, CancellationToken token) =>
        {
            var query = new GetPurchaseLogisticQuery(id);
            var result = await sender.Send(query, token);
            return Results.Ok(new GetPurchaseLogisticResponse(result.PurchaseLogistic));
        }).WithTags("Purchases")
        .WithDescription("Получение логистики закупки")
        .WithDisplayName("Получение логистики закупки")
        .RequireAnyPermission(PermissionCodes.PURCHASE_GET);
    }
}