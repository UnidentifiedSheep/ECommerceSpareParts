using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Balances.CreateTransaction;

public record CreateTransactionRequest(
    string SenderId,
    string ReceiverId,
    decimal Amount,
    int CurrencyId,
    string WhoCreatedTransaction,
    DateTime TransactionDateTime);

public class CreateTransactionEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/balances/transaction", async (ISender sender, CreateTransactionRequest request, 
            CancellationToken token, HttpContext context) =>
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var command = new CreateTransactionCommand(request.SenderId, request.ReceiverId, 
                request.Amount, request.CurrencyId, userId, request.TransactionDateTime);
            await sender.Send(command, token);
            return Results.Ok();
        }).WithGroup("Balances")
        .RequireAuthorization("AMW")
        .WithDescription("Создание транзакции")
        .WithDisplayName("Создание транзакции");
    }
}