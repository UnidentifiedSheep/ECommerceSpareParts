using System.Net.Http.Headers;
using Armtek.Integrations.Core.Interfaces;

namespace Armtek.Integrations.Client;

public abstract class 
    NodeBase(IArmtekConnectionProvider connectionProvider)
{
    protected async Task<HttpRequestMessage> GetRequest(
        HttpMethod method,
        string relativeUrl,
        CancellationToken ct = default)
    {
        var connection = await connectionProvider.GetConnectionAsync(ct);

        var request = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri(connection.BaseUri, relativeUrl)
        };

        /*
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", connection.Auth.GetToken());*/

        return request;
    }
}