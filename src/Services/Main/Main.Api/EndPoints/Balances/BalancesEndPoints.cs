using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Balances;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Balance.ReverseTransaction;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Balances;

public record CreateTransactionRequest(
    Guid SenderId,
    Guid ReceiverId,
    decimal Amount,
    int CurrencyId,
    DateTime TransactionDateTime);

public record GetTransactionsAmwResponse(IEnumerable<TransactionDto> Transactions);

public record GetTransactionsRequest(
    [FromQuery(Name = "rangeStart")] DateTime RangeStart,
    [FromQuery(Name = "rangeEnd")] DateTime RangeEnd,
    [FromQuery(Name = "currencyId")] int? CurrencyId,
    [FromQuery(Name = "senderId")] Guid? SenderId,
    [FromQuery(Name = "receiverId")] Guid? ReceiverId,
    [FromQuery(Name = "page")] int Page,
    [FromQuery(Name = "limit")] int Limit);

public class BalancesEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var balances = app.MapGroup("/balances")
            .WithTags("Balances");

        balances.MapPost("/transaction", async (
                ISender sender,
                CreateTransactionRequest request,
                CancellationToken token) =>
            {
                var command = new CreateTransactionCommand(
                    request.SenderId,
                    request.ReceiverId,
                    request.Amount,
                    request.CurrencyId,
                    request.TransactionDateTime);
                await sender.Send(command, token);
                return Results.Ok();
            })
            .WithDescription("Создание транзакции")
            .WithDisplayName("Создание транзакции")
            .RequireAnyPermission(PermissionCodes.BALANCES_TRANSACTION_CREATE);

        balances.MapDelete("/transaction/{id}", async (
                ISender sender,
                IUserContext user,
                Guid id,
                CancellationToken token) =>
            {
                await sender.Send(new ReverseTransactionCommand(id, user.UserId), token);
                return Results.Ok();
            })
            .WithDescription("Удалить транзакцию")
            .WithDisplayName("Удалить транзакцию")
            .RequireAnyPermission(PermissionCodes.BALANCES_TRANSACTION_DELETE);

        balances.MapGet("/transactions", async (
                ISender sender,
                [AsParameters] GetTransactionsRequest request,
                CancellationToken token) =>
            {
                await Task.CompletedTask;
            })
            .WithDescription("Получение списка транзакций")
            .WithDisplayName("Получение транзакций")
            .RequireAnyPermission(PermissionCodes.BALANCES_TRANSACTION_GET_ALL);
    }
}
