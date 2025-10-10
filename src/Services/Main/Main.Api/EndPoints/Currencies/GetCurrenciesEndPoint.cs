using Carter;
using Core.Models;
using Main.Application.Handlers.Currencies.GetCurrencies;
using Main.Core.Dtos.Currencies;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Currencies;

public record GetCurrenciesResponse(IEnumerable<CurrencyDto> Currencies);

public class GetCurrenciesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/currencies", async (ISender sender, int page, int limit, CancellationToken cancellation) =>
            {
                var query = new GetCurrenciesQuery(new PaginationModel(page, limit));
                var result = await sender.Send(query, cancellation);
                return Results.Ok(result.Adapt<GetCurrenciesResponse>());
            }).WithTags("Currencies")
            .WithDescription("Получение валют")
            .WithDisplayName("Получение Валют");
    }
}