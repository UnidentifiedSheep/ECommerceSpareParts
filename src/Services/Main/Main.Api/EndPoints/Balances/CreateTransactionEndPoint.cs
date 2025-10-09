using System.Security.Claims;
using Carter;
using Core.Enums;
using Main.Application.Handlers.Balance.CreateTransaction;
using MediatR;

namespace Main.Api.EndPoints.Balances;

public record CreateTransactionRequest(
    Guid SenderId,
    Guid ReceiverId,
    decimal Amount,
    int CurrencyId,
    DateTime TransactionDateTime);

public class CreateTransactionEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/balances/transaction", async (ISender sender, CreateTransactionRequest request,
                CancellationToken token, ClaimsPrincipal claims) =>
            {
                if (!Guid.TryParse(claims.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Results.Unauthorized();
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