using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Balances;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Balance.GetTransactions;
using Main.Application.Handlers.Balance.ReverseTransaction;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints;

public record CreateTransactionRequest(
    Guid SenderId,
    Guid ReceiverId,
    decimal Amount,
    int CurrencyId,
    DateTime TransactionDateTime);

public record GetTransactionsResponse(IReadOnlyList<TransactionDto> Transactions);

public record GetTransactionsRequest(
    [FromQuery(Name = "rangeStart")] DateTime RangeStart,
    [FromQuery(Name = "rangeEnd")] DateTime RangeEnd,
    [FromQuery(Name = "currencyId")] int? CurrencyId,
    [FromQuery(Name = "senderId")] Guid? SenderId,
    [FromQuery(Name = "receiverId")] Guid? ReceiverId,
    [FromQuery(Name = "logicalOperator")] LogicalOperation LogicalOperation,
    [FromQuery(Name = "cursorId")] Guid? CursorId,
    [FromQuery(Name = "cursorDate")] DateTime? CursorDate,
    [FromQuery(Name = "size")] int Size);

public class BalancesEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var balances = app.MapGroup("/balances")
            .WithTags("Balances");

        balances.MapPost("/transactions", async (
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
            .WithName("CreateBalanceTransaction")
            .WithSummary("Создать транзакцию баланса")
            .WithDescription("Создание транзакции")
            .WithDisplayName("Создание транзакции")
            .Accepts<CreateTransactionRequest>(false, "application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.BALANCES_TRANSACTION_CREATE);

        balances.MapDelete("/transactions/{id:guid}", async (
                ISender sender,
                Guid id,
                CancellationToken token) =>
            {
                await sender.Send(new ReverseTransactionCommand(id), token);
                return Results.Ok();
            })
            .WithName("DeleteBalanceTransaction")
            .WithSummary("Отменить транзакцию баланса")
            .WithDescription("Удалить транзакцию")
            .WithDisplayName("Удалить транзакцию")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.BALANCES_TRANSACTION_DELETE);

        balances.MapGet("/transactions", async (
                ISender sender,
                [AsParameters] GetTransactionsRequest request,
                CancellationToken token) =>
            {
                var query = new GetTransactionsQuery(
                    request.RangeStart,
                    request.RangeEnd,
                    request.CurrencyId,
                    request.SenderId,
                    request.ReceiverId,
                    request.LogicalOperation,
                    new Cursor<(Guid id, DateTime dt)>((
                            request.CursorId ?? Guid.Empty,
                            request.CursorDate ?? DateTime.MinValue),
                        request.Size));

                var result = await sender.Send(query, token);
                return Results.Ok(new GetTransactionsResponse(result.Transactions));
            })
            .WithName("GetBalanceTransactions")
            .WithSummary("Получить транзакции баланса")
            .WithDescription("Получение списка транзакций")
            .WithDisplayName("Получение транзакций")
            .Produces<GetTransactionsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.BALANCES_TRANSACTION_GET_ALL);
    }
}
