using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using Main.Application.Handlers.Balance.GetTransactions;
using Main.Core.Dtos.Amw.Balances;
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
        app.MapGet("/balances/transactions", async (ISender sender, ClaimsPrincipal user,
                [AsParameters] GetTransactionsRequest request, CancellationToken token) =>
            {
                var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                if (roles.IsAnyMatchInvariant("admin", "moderator", "worker"))
                    return await GetAmw(sender, request, token);
                return null;
            }).WithTags("Balances")
            .WithDescription("Получение списка транзакций")
            .WithDisplayName("Получение транзакций");
    }

    private async Task<IResult> GetAmw(ISender sender, GetTransactionsRequest request, CancellationToken token)
    {
        var query = request.Adapt<GetTransactionsQuery>();
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetTransactionsAmwResponse>();
        return Results.Ok(response);
    }
}