using Carter;

namespace Main.Api.EndPoints.Internal;

public class InternalEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/internal")
            .WithTags("Internal")
            .AddInternalAuthEndPoints()
            .AddInternalProducerEndPoints()
            .AddInternalProductsEndPoints()
            .AddInternalPurchaseEndPoints()
            .AddInternalSaleEndPoints()
            .AddInternalCurrencyEndPoints();
    }
}