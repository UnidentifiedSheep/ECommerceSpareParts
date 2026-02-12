using System.Security.Claims;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Enums;
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
                CancellationToken token, IUserContext user) =>
            {
                var command = new CreateTransactionCommand(request.SenderId, request.ReceiverId,
                    request.Amount, request.CurrencyId, user.UserId, request.TransactionDateTime, TransactionStatus.Normal);
                await sender.Send(command, token);
                return Results.Ok();
            }).WithTags("Balances")
            .WithDescription("Создание транзакции")
            .WithDisplayName("Создание транзакции")
            .RequireAnyPermission("BALANCES.TRANSACTION.CREATE");
    }
}