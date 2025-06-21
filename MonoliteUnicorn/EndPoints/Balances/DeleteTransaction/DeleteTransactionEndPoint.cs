using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Balances.DeleteTransaction;

public class DeleteTransactionEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/balances/transaction/{id}",
            async (ISender sender, HttpContext context, string id, CancellationToken token) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                var command = new DeleteTransactionCommand(id, userId);
                await sender.Send(command, token);
                return Results.Ok();
            }).WithGroup("Balances")
            .RequireAuthorization("AMW")
            .WithDescription("Удалить транзакцию")
            .WithDisplayName("Удалить транзакцию");
    }
}