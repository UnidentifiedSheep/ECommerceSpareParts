using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Sales.GetSale;
using MediatR;

namespace Main.Api.EndPoints.Transactions;

public record GetTransactionSaleResponse(SaleDto Sale);

public static class TransactionSaleEndPoints
{
    public static RouteGroupBuilder MapTransactionSaleEndPoints(this RouteGroupBuilder balances)
    {
        balances.MapGet("{transactionId:guid}/sale", async (
                ISender sender,
                Guid transactionId,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetSaleQuery(null, transactionId),
                    cancellationToken);

                return Results.Ok(new GetTransactionSaleResponse(result.Sale));
            })
            .WithTags("Transactions")
            .WithName("GetSaleByTransaction")
            .WithSummary("Получить продажу по транзакции")
            .WithDescription("Получение продажи, связанной с транзакцией")
            .WithDisplayName("Получение продажи по транзакции")
            .Produces<GetTransactionSaleResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.SALES_GET);

        return balances;
    }
}
