using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Currencies.GetCurrencies;
using Main.Application.Handlers.Currencies.GetCurrencyById;
using Main.Abstractions.Dtos.Currencies;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Currencies;

public record GetCurrenciesResponse(IEnumerable<CurrencyDto> Currencies);
public record GetCurrencyByIdResponse(CurrencyDto Currency);

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
            .WithDescription("Получение списка валют")
            .WithDisplayName("Получение списка валют")
            .RequireAnyPermission("CURRENCIES.GET");
        
        app.MapGet("/currencies/{id}", async (ISender sender, int id, CancellationToken cancellation) =>
            {
                var command = new GetCurrencyByIdQuery(id);
                var result = await sender.Send(command, cancellation);
                return Results.Ok(result.Adapt<GetCurrencyByIdResponse>());
            }).WithTags("Currencies")
            .WithDescription("Получение валюты по идентификатору")
            .WithDisplayName("Получение валюты по id")
            .RequireAnyPermission("CURRENCIES.GET");
    }
}