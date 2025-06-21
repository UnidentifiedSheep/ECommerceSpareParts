using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Sales.DeleteSale;

public class DeleteSaleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/sales/{saleId}",
            async (ISender sender, ClaimsPrincipal claims, string saleId, CancellationToken token) =>
            {
                var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Results.Unauthorized();
                var command = new DeleteSaleCommand(saleId, userId);
                await sender.Send(command, token);
                return Results.NoContent();
            }).RequireAuthorization("AMW")
            .WithGroup("Sales")
            .WithDescription("Удаление продажи")
            .WithDisplayName("Удаление продажи");
    }
}