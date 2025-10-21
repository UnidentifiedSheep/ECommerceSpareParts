using System.Security.Claims;
using Carter;
using Main.Application.Handlers.Purchases.DeleteFullPurchase;
using MediatR;

namespace Main.Api.EndPoints.Purchase;

public class DeletePurchaseEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/purchases/{purchaseId}", async (ISender sender, string purchaseId,
                ClaimsPrincipal claims, CancellationToken cancellationToken) =>
            {
                if (!Guid.TryParse(claims.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Results.Unauthorized();
                var command = new DeleteFullPurchaseCommand(purchaseId, userId);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("Purchases")
            .WithDescription("Удаление закупки")
            .WithDisplayName("Удаление закупки");
    }
}