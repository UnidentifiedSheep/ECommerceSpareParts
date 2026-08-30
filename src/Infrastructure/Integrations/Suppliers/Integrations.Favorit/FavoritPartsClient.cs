using Abstractions.Models.Options;
using Integrations.Client.Core;
using Integrations.Common;
using Integrations.Favorit.Requests;
using Integrations.Favorit.Responses;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Interfaces;

namespace Integrations.Favorit;

public interface IFavoritPartsClient
{
	Task<Response<GetPricesResponse>> GetPricesAsync(
		GetPricesRequest request,
		CancellationToken token = default);
}

public class FavoritPartsClient(
	HttpClient client,
	IConnectionProvider<FavoritConnection> connectionProvider,
	ProjectJsonOptions jsonOptions) : ClientBase(jsonOptions), IFavoritPartsClient
{
	public async Task<Response<GetPricesResponse>> GetPricesAsync(
		GetPricesRequest request,
		CancellationToken token = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(request.Number);
		var connection = await connectionProvider.GetConnectionAsync(token);

		var httpRequest = new HttpRequestMessage
		{
			Method = HttpMethod.Get, RequestUri = new Uri(new Uri(connection.BaseUrl), "/hs/hsprice")
		};

		var @params = new Dictionary<string, string?>
		{
			["number"] = request.Number, ["key"] = connection.ApiKey
		};

		if (!string.IsNullOrWhiteSpace(request.Brand))
			@params.Add("brand", request.Brand);
		if (request.ShowAnalogues)
			@params.Add("analogues", "on");
		if (request.ShowIsRefundable)
			@params.Add("info", "on");

		AddQueryParameters(httpRequest, @params);
		var response = await client.SendAsync(httpRequest, token);
		return await ReadResponse<GetPricesResponse>(response, token);
	}
}
