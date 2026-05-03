using Abstractions.Models;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Main.Application.Dtos.Currencies;
using Main.Application.Handlers.Currencies.GetCurrencies;
using Main.Application.Handlers.Currencies.GetCurrencyById;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Currencies;

public record GetCurrenciesResponse(IReadOnlyList<CurrencyDto> Currencies);

public record GetCurrencyByIdResponse(CurrencyDto Currency);

public class GetCurrenciesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/currencies", async (ISender sender, PaginationQueryModel queryParams, CancellationToken cancellation) =>
            {
                var query = new GetCurrenciesQuery(queryParams);
                var result = await sender.Send(query, cancellation);
                return Results.Ok(new GetCurrenciesResponse(result.Currencies));
            }).WithTags("Currencies")
            .WithDescription("Получение списка валют")
            .WithDisplayName("Получение списка валют")
            .RequireAnyPermission("CURRENCIES.GET");

        app.MapGet("/currencies/{id}", async (ISender sender, int id, CancellationToken cancellation) =>
            {
                var command = new GetCurrencyByIdQuery(id);
                var result = await sender.Send(command, cancellation);
                return Results.Ok(new GetCurrencyByIdResponse(result.Currency));
            }).WithTags("Currencies")
            .WithDescription("Получение валюты по идентификатору")
            .WithDisplayName("Получение валюты по id")
            .RequireAnyPermission("CURRENCIES.GET");
    }
}