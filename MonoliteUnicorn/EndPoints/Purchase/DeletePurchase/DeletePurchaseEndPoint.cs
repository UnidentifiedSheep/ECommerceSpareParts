using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Purchase.DeletePurchase;

public class DeletePurchaseEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/purchases/{purchaseId}", async (ISender sender, string purchaseId,
            ClaimsPrincipal claims, CancellationToken cancellationToken) =>
        {
            var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();
            var command = new DeletePurchaseCommand(purchaseId, userId);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).WithGroup("Purchases")
        .RequireAuthorization("AMW")
        .WithDescription("Удаление закупки")
        .WithDisplayName("Удаление закупки");
    }
}