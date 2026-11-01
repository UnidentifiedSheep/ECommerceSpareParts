using Api.Common.Extensions;
using Enums;
using System.Text.Json.Serialization;
using Main.Application.Handlers.Currencies.GetCurrencyRate;
using MediatR;

namespace Main.Api.EndPoints.Internal;

public record GetCurrencyRateResult
{
	[JsonPropertyName("rate")]
	public required decimal Rate { get; init; }
}

public static class InternalCurrencyEndPoints
{
	public static RouteGroupBuilder AddInternalCurrencyEndPoints(this RouteGroupBuilder group)
	{
		var currency = group
			.MapGroup("/currencies")
			.WithGroupName("Internal Currency")
			.WithTags("InternalCurrencies");

		currency
			.MapGet(
				"{id:int}/rates/",
				async (
					ISender sender, int id,
					CancellationToken cancellationToken) =>
				{
					var query = new GetCurrencyRateQuery(id);
					var result = await sender.Send(query, cancellationToken);
					return Results.Ok(
						new GetCurrencyRateResult
						{
							Rate = result.Rate
						});
				})
			.WithDisplayName("Internal service currency rates")
			.WithName("InternalCurrencyRates")
			.WithSummary("Получить курс валюты для внутреннего сервиса")
			.WithDescription("Получение курса валюты по id для внутренних интеграций")
			.Produces<GetCurrencyRateResult>()
			.ProducesProblem(StatusCodes.Status404NotFound);

		currency.MapGet("{id:int}/rates/historical", async (
			ISender sender, int id, DateTimeOffset forDate, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetHistoricalCurrencyRateQuery(id, forDate), cancellationToken);
			return Results.Ok(new GetCurrencyRateResult { Rate = result.Rate });
		})
			.WithName("InternalHistoricalCurrencyRate")
			.WithSummary("Получить исторический курс валюты для внутреннего сервиса")
			.Produces<GetCurrencyRateResult>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.RequireAnyRole(Role.System);

		return group;
	}
}
