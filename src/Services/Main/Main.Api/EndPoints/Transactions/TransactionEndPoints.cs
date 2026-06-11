using System.Text.Json.Serialization;
using Abstractions.Models;
using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Balances;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Balance.GetTransactions;
using Main.Application.Handlers.Balance.ReverseTransaction;
using Main.Enums.Balances;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Transactions;

public record CreateTransactionRequest
{
    [JsonPropertyName("senderId")]
    public Guid SenderId { get; init; }
    [JsonPropertyName("receiverId")]
    public Guid ReceiverId { get; init; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }
    [JsonPropertyName("currencyId")]
    public int CurrencyId { get; init; }
    [JsonPropertyName("transactionDateTime")]
    public DateTime TransactionDateTime { get; init; }
}

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
    [FromQuery(Name = "size")] int Size,
    [FromQuery(Name = "skipReversed")] bool SkipReversed);

public static class TransactionEndPoints
{
    public static RouteGroupBuilder MapTransactionEndPoints(this RouteGroupBuilder balances)
    {
        balances.MapPost("", async (
                ISender sender,
                CreateTransactionRequest request,
                CancellationToken token) =>
            {
                var command = new CreateTransactionCommand(
                    request.SenderId,
                    request.ReceiverId,
                    request.Amount,
                    request.CurrencyId,
                    request.TransactionDateTime,
                    TransactionSourceType.Manual);
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

        balances.MapDelete("{id:guid}", async (
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

        balances.MapGet("", async (
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
                        request.Size),
                    request.SkipReversed);

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

        return balances;
    }
}
