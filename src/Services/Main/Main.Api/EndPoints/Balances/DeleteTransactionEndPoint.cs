using System.Security.Claims;
using Carter;
using Main.Application.Handlers.Balance.DeleteTransaction;
using MediatR;

namespace Main.Api.EndPoints.Balances;

public class DeleteTransactionEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/balances/transaction/{id}",
                async (ISender sender, ClaimsPrincipal claims, string id, CancellationToken token) =>
                {
                    if (!Guid.TryParse(claims.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                        return Results.Unauthorized();
                    var command = new DeleteTransactionCommand(id, userId);
                    await sender.Send(command, token);
                    return Results.Ok();
                }).WithTags("Balances")
            .RequireAuthorization("AMW")
            .WithDescription("Удалить транзакцию")
            .WithDisplayName("Удалить транзакцию");
    }
}