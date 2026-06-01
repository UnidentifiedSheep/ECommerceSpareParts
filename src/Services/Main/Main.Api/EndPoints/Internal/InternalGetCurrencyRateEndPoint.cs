using System.Text.Json.Serialization;
using Carter;
using Main.Application.Handlers.Currencies.GetCurrencyRate;
using MediatR;

namespace Main.Api.EndPoints.Internal.Currency;

public record GetCurrencyRateResult
{
    [JsonPropertyName("rate")]
    public required decimal Rate { get; init; }
}

public class InternalGetCurrencyRateEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/internal/currencies/{id:int}/rates/", async (
                ISender sender, 
                int id, 
                CancellationToken cancellationToken) =>
            {
                var query = new GetCurrencyRateQuery(id);
                var result = await sender.Send(query, cancellationToken);
                return Results.Ok(new GetCurrencyRateResult
                {
                    Rate = result.Rate
                });
            }).WithGroupName("Internal Currency")
        .WithDisplayName("Internal service currency rates")
        .WithName("InternalCurrencyRates")
        .WithSummary("Получить курс валюты для внутреннего сервиса")
        .WithDescription("Получение курса валюты по id для внутренних интеграций")
        .Produces<GetCurrencyRateResult>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
