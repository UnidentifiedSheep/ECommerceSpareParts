using System.Security.Claims;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Purchases.DeleteFullPurchase;
using MediatR;

namespace Main.Api.EndPoints.Purchase;

public class DeletePurchaseEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/purchases/{purchaseId}", async (ISender sender, string purchaseId,
                IUserContext user, CancellationToken cancellationToken) =>
            {
                var command = new DeleteFullPurchaseCommand(purchaseId, user.UserId);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("Purchases")
            .WithDescription("Удаление закупки")
            .WithDisplayName("Удаление закупки")
            .RequireAnyPermission("PURCHASE.DELETE");
    }
}