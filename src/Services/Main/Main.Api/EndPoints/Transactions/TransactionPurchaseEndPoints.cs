using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Purchase;
using Main.Application.Handlers.Purchases.GetPurchase;
using MediatR;

namespace Main.Api.EndPoints.Transactions;

public record GetTransactionPurchaseResponse(PurchaseDto Purchase);

public static class TransactionPurchaseEndPoints
{
    public static RouteGroupBuilder MapTransactionPurchaseEndPoints(this RouteGroupBuilder balances)
    {
        balances.MapGet("{transactionId:guid}/purchase", async (
                ISender sender,
                Guid transactionId,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetPurchaseQuery(null, transactionId),
                    cancellationToken);

                return Results.Ok(new GetTransactionPurchaseResponse(result.Purchase));
            })
            .WithTags("Transactions")
            .WithName("GetPurchaseByTransaction")
            .WithSummary("Получить закупку по транзакции")
            .WithDescription("Получение закупки, связанной с транзакцией")
            .WithDisplayName("Получение закупки по транзакции")
            .Produces<GetTransactionPurchaseResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.PURCHASE_GET);

        return balances;
    }
}
