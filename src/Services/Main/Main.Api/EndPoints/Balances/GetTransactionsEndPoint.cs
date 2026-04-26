using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Main.Application.Dtos.Amw.Balances;
using Main.Application.Handlers.Balance.GetTransactions;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Balances;

public record GetTransactionsAmwResponse(IEnumerable<TransactionDto> Transactions);

public record GetTransactionsRequest(
    [FromQuery(Name = "rangeStart")]
    DateTime RangeStart,
    [FromQuery(Name = "rangeEnd")]
    DateTime RangeEnd,
    [FromQuery(Name = "currencyId")]
    int? CurrencyId,
    [FromQuery(Name = "senderId")]
    Guid? SenderId,
    [FromQuery(Name = "receiverId")]
    Guid? ReceiverId,
    [FromQuery(Name = "page")]
    int Page,
    [FromQuery(Name = "limit")]
    int Limit);

public class GetTransactionsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/balances/transactions", async (
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
                    new Pagination(request.Page, request.Limit));
                var result = await sender.Send(query, token);
                var response = result.Adapt<GetTransactionsAmwResponse>();
                return Results.Ok(response);
            }).WithTags("Balances")
            .WithDescription("Получение списка транзакций")
            .WithDisplayName("Получение транзакций")
            .RequireAnyPermission("BALANCES.TRANSACTION.GET.ALL");
    }
}