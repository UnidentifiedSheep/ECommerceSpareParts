using Carter;

namespace Main.Api.EndPoints.Transactions;

public class TransactionsEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var balances = app.MapGroup("/transactions")
            .WithTags("Transactions");

        balances.MapTransactionEndPoints()
            .MapTransactionPurchaseEndPoints()
            .MapTransactionSaleEndPoints();
    }
}