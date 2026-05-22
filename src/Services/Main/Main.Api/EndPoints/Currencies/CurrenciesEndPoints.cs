using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Currencies;
using Main.Application.Handlers.Currencies.CreateCurrency;
using Main.Application.Handlers.Currencies.GetCurrencies;
using Main.Application.Handlers.Currencies.GetCurrencyById;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Currencies;

public record CreateCurrencyRequest(string ShortName, string Name, string CurrencySign, string Code);

public record CreateCurrencyResponse(int Id);

public record GetCurrenciesResponse(IReadOnlyList<CurrencyDto> Currencies);

public record GetCurrencyByIdResponse(CurrencyDto Currency);

public class CurrenciesEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var currencies = app.MapGroup("/currencies")
            .WithTags("Currencies");

        currencies.MapPost("", async (
                ISender sender,
                CreateCurrencyRequest request,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(request.Adapt<CreateCurrencyCommand>(), cancellationToken);
                return Results.Created($"currencies/{result.Id}", new CreateCurrencyResponse(result.Id));
            })
            .WithDescription("Создание валюты")
            .WithDisplayName("Создание валюты")
            .RequireAnyPermission(PermissionCodes.CURRENCIES_CREATE);

        currencies.MapGet("", async (
                ISender sender,
                [AsParameters] PaginationQueryModel queryParams,
                CancellationToken cancellation) =>
            {
                var result = await sender.Send(new GetCurrenciesQuery(queryParams), cancellation);
                return Results.Ok(new GetCurrenciesResponse(result.Currencies));
            })
            .WithDescription("Получение списка валют")
            .WithDisplayName("Получение списка валют")
            .RequireAnyPermission(PermissionCodes.CURRENCIES_GET);

        currencies.MapGet("/{id}", async (ISender sender, int id, CancellationToken cancellation) =>
            {
                var result = await sender.Send(new GetCurrencyByIdQuery(id), cancellation);
                return Results.Ok(new GetCurrencyByIdResponse(result.Currency));
            })
            .WithDescription("Получение валюты по идентификатору")
            .WithDisplayName("Получение валюты по id")
            .RequireAnyPermission(PermissionCodes.CURRENCIES_GET);
    }
}
