using System.Text.Json.Serialization;
using Integrations.Common;
using Internal.Integration.Core;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main.Producer;
using Microsoft.Extensions.Options;

namespace Internal.Integration.Main;

internal sealed class ProducerNode(
    HttpClient httpClient,
    IAuthClient authClient,
    IOptionsMonitor<InternalServiceCredentials> optionsMonitor
) : InternalClientBase(authClient, optionsMonitor), IProducerNode
{
    public async Task<Response<IReadOnlyList<InternalFullProducer>>> GetFullProducer(
        IEnumerable<int> producerIds,
        CancellationToken cancellationToken = default)
    {
        var ids = producerIds.Select(x => x.ToString()).ToArray();
        using var request = await GetRequest(
            HttpMethod.Get,
            $"/internal/producers{IdsAsQueryString(ids, "id")}",
            cancellationToken);

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        return await ReadResponse<GetFullProducersResponse, IReadOnlyList<InternalFullProducer>>(
            response,
            x => x.Producers,
            cancellationToken);
    }

    private record GetFullProducersResponse
    {
        [JsonPropertyName("producers")]
        public IReadOnlyList<InternalFullProducer> Producers { get; init; } = [];
    }
}