using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Currencies;
using Main.Application.Handlers.Currencies;
using Main.Application.Handlers.Currencies.CreateCurrency;
using Main.Application.Handlers.Currencies.GetCurrencyHistory;
using Main.Application.Handlers.Currencies.UpdateCurrenciesRates;
using MediatR;

namespace Main.Api.EndPoints;

public record CreateCurrencyRequest(
    string ShortName,
    string Name,
    string CurrencySign,
    string Code
);

public record CreateCurrencyResponse(int Id);

public record GetCurrenciesResponse(IReadOnlyList<CurrencyDto> Currencies);

public record GetCurrencyByIdResponse(CurrencyDto Currency);

public record GetCurrencyHistoryResponse(IReadOnlyList<CurrencyRateHistoryDto> History);

public class CurrenciesEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var currencies = app.MapGroup("/currencies")
            .WithTags("Currencies");

        currencies.MapPost(
                "",
                async (
                    ISender sender,
                    CreateCurrencyRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new CreateCurrencyCommand(
                            request.ShortName,
                            request.Name,
                            request.CurrencySign,
                            request.Code),
                        cancellationToken);
                    return Results.Created($"currencies/{result.Id}", new CreateCurrencyResponse(result.Id));
                })
            .WithName("CreateCurrency")
            .WithSummary("Создать валюту")
            .WithDescription("Создание валюты")
            .WithDisplayName("Создание валюты")
            .Accepts<CreateCurrencyRequest>(false, "application/json")
            .Produces<CreateCurrencyResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.CURRENCIES_CREATE);

        currencies.MapGet(
                "",
                async (
                    ISender sender,
                    CancellationToken cancellation) =>
                {
                    var result = await sender.Send(new GetAllCurrenciesQuery(), cancellation);
                    return Results.Ok(new GetCurrenciesResponse(result.Currencies));
                })
            .WithName("GetCurrencies")
            .WithSummary("Получить валюты")
            .WithDescription("Получение списка валют")
            .WithDisplayName("Получение списка валют")
            .Produces<GetCurrenciesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.CURRENCIES_GET);

        currencies.MapGet(
                "/{id:int}",
                async (
                    ISender sender,
                    int id,
                    CancellationToken cancellation) =>
                {
                    var result = await sender.Send(new GetCurrencyByIdQuery(id), cancellation);
                    return Results.Ok(new GetCurrencyByIdResponse(result.Currency));
                })
            .WithName("GetCurrencyById")
            .WithSummary("Получить валюту по id")
            .WithDescription("Получение валюты по идентификатору")
            .WithDisplayName("Получение валюты по id")
            .Produces<GetCurrencyByIdResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.CURRENCIES_GET);

        currencies.MapGet(
                "/{id:int}/history",
                async (
                    ISender sender,
                    int id,
                    [AsParameters] PaginationQueryModel queryParams,
                    CancellationToken cancellation) =>
                {
                    var result = await sender.Send(
                        new GetCurrencyHistoryQuery(id, queryParams),
                        cancellation);
                    return Results.Ok(new GetCurrencyHistoryResponse(result.History));
                })
            .WithName("GetCurrencyHistory")
            .WithSummary("Получить историю курса валюты")
            .WithDescription("Получение истории изменений курса валюты по id")
            .WithDisplayName("История курса валюты")
            .Produces<GetCurrencyHistoryResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.CURRENCIES_GET);

        currencies.MapPost(
                "/update",
                async (ISender sender, CancellationToken token) =>
                {
                    await sender.Send(new UpdateCurrenciesRatesCommand(), token);
                    return Results.Ok();
                })
            .WithName("UpdateCurrencyRates")
            .WithSummary("Обновление курсов валют")
            .WithDescription("Обновление курсов валют")
            .WithDisplayName("Обновление курсов валют")
            .RequireAnyPermission(PermissionCodes.CURRENCIES_CREATE);
    }
}