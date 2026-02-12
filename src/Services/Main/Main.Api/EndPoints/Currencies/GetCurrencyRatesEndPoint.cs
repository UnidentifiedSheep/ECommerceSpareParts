using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.Currencies.GetCurrencyRates;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Currencies;

public record GetCurrencyRatesResponse(Dictionary<int, decimal> Rates);

public class GetCurrencyRatesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/currencies/rates", async (ISender sender, CancellationToken token) =>
        {
            var query = new GetCurrencyRatesQuery();
            var result = await sender.Send(query, token);
            return Results.Ok(new GetCurrencyRatesResponse(result.Rates));
        }).WithTags("Currencies")
        .WithDescription("Получение курсов валют к доллару")
        .WithDisplayName("Получение курсов валют к доллару")
        .RequireAnyPermission(PermissionCodes.CURRENCIES_GET);
    }
}