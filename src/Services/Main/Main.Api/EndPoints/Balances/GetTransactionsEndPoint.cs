using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Balance.GetTransactions;
using Main.Abstractions.Dtos.Amw.Balances;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Balances;

public record GetTransactionsAmwResponse(IEnumerable<TransactionDto> Transactions);

public record GetTransactionsRequest(
    [FromQuery(Name = "rangeStart")] DateTime RangeStart,
    [FromQuery(Name = "rangeEnd")] DateTime RangeEnd,
    [FromQuery(Name = "currencyId")] int? CurrencyId,
    [FromQuery(Name = "senderId")] string? SenderId,
    [FromQuery(Name = "receiverId")] string? ReceiverId,
    [FromQuery(Name = "page")] int Page,
    [FromQuery(Name = "limit")] int Limit);

public class GetTransactionsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/balances/transactions", async (ISender sender, 
                [AsParameters] GetTransactionsRequest request, CancellationToken token) =>
            {
                var query = request.Adapt<GetTransactionsQuery>();
                var result = await sender.Send(query, token);
                var response = result.Adapt<GetTransactionsAmwResponse>();
                return Results.Ok(response);
            }).WithTags("Balances")
            .WithDescription("Получение списка транзакций")
            .WithDisplayName("Получение транзакций")
            .RequireAnyPermission("BALANCES.TRANSACTION.GET.ALL");
    }
}