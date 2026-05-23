using Carter;

namespace Search.Api.EndPoints.Products;

public class SearchProductsEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/products")
            .WithTags("Products")
            .AddSearchProductsByAll()
            .SearchProductsBySku();
    }
}