using System.Security.Claims;
using Carter;
using Main.Application.Handlers.Sales.DeleteFullSale;
using MediatR;

namespace Main.Api.EndPoints.Sales;

public class DeleteSaleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/sales/{saleId}",
                async (ISender sender, ClaimsPrincipal claims, string saleId, CancellationToken token) =>
                {
                    if (!Guid.TryParse(claims.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                        return Results.Unauthorized();
                    var command = new DeleteFullSaleCommand(saleId, userId);
                    await sender.Send(command, token);
                    return Results.NoContent();
                }).RequireAuthorization("AMW")
            .WithTags("Sales")
            .WithDescription("Удаление продажи")
            .WithDisplayName("Удаление продажи");
    }
}