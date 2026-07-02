using Carter;

namespace Search.Api.EndPoints.Producers;

public class SearchProducersEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/producers")
            .WithTags("Producers")
            .SearchProducers()
            .GetProducerAliases();
    }
}