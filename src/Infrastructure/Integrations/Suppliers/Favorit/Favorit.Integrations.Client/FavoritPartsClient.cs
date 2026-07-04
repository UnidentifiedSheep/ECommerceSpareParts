using Favorit.Integrations.Core.Interfaces;
using Favorit.Integrations.Core.Requests;
using Favorit.Integrations.Core.Responses;
using Integrations.Client.Core;
using Integrations.Common;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Interfaces;
using Microsoft.AspNetCore.WebUtilities;

namespace Favorit.Integrations.Client;

public class FavoritPartsClient(
    HttpClient client,
    IConnectionProvider<FavoritConnection> connectionProvider
) : ClientBase, IFavoritPartsClient
{
    public async Task<Response<GetPricesResponse>> GetPricesAsync(
        GetPricesRequest request,
        CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Number);
        var connection = await connectionProvider.GetConnectionAsync(token);

        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(new Uri(connection.BaseUrl), "/hs/hsprice")
        };

        var @params = new Dictionary<string, string?>
        {
            ["number"] = request.Number,
            ["key"] = connection.ApiKey
        };

        if (!string.IsNullOrWhiteSpace(request.Brand)) @params.Add("brand", request.Brand);
        if (request.ShowAnalogues) @params.Add("analogues", "on");
        if (request.ShowIsRefundable) @params.Add("info", "on");

        httpRequest.RequestUri =
            new Uri(QueryHelpers.AddQueryString(httpRequest.RequestUri!.ToString(), @params));
        var response = await client.SendAsync(httpRequest, token);
        return await ReadResponse<GetPricesResponse>(response, token);
    }
}