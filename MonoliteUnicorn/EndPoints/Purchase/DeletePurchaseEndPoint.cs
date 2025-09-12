using System.Security.Claims;
using Application.Handlers.Purchases.DeleteFullPurchase;
using Carter;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Purchase;

public class DeletePurchaseEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/purchases/{purchaseId}", async (ISender sender, string purchaseId,
            ClaimsPrincipal claims, CancellationToken cancellationToken) =>
        {
            var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();
            var command = new DeleteFullPurchaseCommand(purchaseId, userId);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).WithTags("Purchases")
        .RequireAuthorization("AMW")
        .WithDescription("Удаление закупки")
        .WithDisplayName("Удаление закупки");
    }
}