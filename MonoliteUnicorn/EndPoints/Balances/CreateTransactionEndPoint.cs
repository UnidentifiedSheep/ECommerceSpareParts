using System.Security.Claims;
using Application.Handlers.Balance.CreateTransaction;
using Carter;
using Core.Enums;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Balances;

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
                    request.Amount, request.CurrencyId, userId, request.TransactionDateTime, TransactionStatus.Normal);
                await sender.Send(command, token);
                return Results.Ok();
            }).WithTags("Balances")
            .RequireAuthorization("AMW")
            .WithDescription("Создание транзакции")
            .WithDisplayName("Создание транзакции");
    }
}