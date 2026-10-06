using System.Text.Json;
using System.Text.Json.Serialization;
using Enums;
using Integrations.Client.Core;
using Integrations.Common;
using Integrations.ExchangeRate.Interfaces;
using Integrations.ExchangeRate.Models;

namespace Integrations.ExchangeRate.MoneyConvert;

public sealed class Client(HttpClient client) :
	ClientBase(new JsonSerializerOptions(JsonSerializerDefaults.Web)),
	IExchangeRateClient
{
	public ExchangeRateProvider Provider => ExchangeRateProvider.MoneyConvert;

	public async Task<Response<ExchangeRates>> GetRates(CancellationToken cancellationToken = default)
	{
		var response = await client.GetAsync("", cancellationToken);
		var result = await ReadResponse<MoneyConvertRatesResponse>(response, cancellationToken);
		return result.Success
			? Response<ExchangeRates>.Ok(new ExchangeRates(result.ValueOrThrow.Base, result.ValueOrThrow.Rates))
			: Response<ExchangeRates>.FromFail(result);
	}

	private sealed record MoneyConvertRatesResponse
	{
		[JsonPropertyName("base")]
		public required string Base { get; init; }

		[JsonPropertyName("disclaimer")]
		public required string Disclaimer { get; init; }

		[JsonPropertyName("license")]
		public required string License { get; init; }

		[JsonPropertyName("ts")]
		public long Timestamp { get; init; }

		[JsonPropertyName("source")]
		public required string Source { get; init; }

		[JsonPropertyName("rates")]
		public Dictionary<string, decimal> Rates { get; init; } = [];
	}

}
