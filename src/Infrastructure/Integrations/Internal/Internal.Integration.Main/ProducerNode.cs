using System.Net;
using System.Text.Json;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Main;

internal sealed class ProducerNode(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor)
    : InternalClientBase(authClient, optionsMonitor), IProducerNode
{
    public async Task<InternalFullProducer?> GetFullProducer(
        int producerId,
        CancellationToken cancellationToken = default)
    {
        using var request = await GetRequest(
            HttpMethod.Get,
            $"/internal/producers/{producerId}",
            cancellationToken);
        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<InternalFullProducer>(json);
    }
}
