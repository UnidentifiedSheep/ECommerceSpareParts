using System.Net;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main.Producer;
using Search.Application.Interfaces.Producer;
using Search.Entities;

namespace Search.Persistence.DocumentProviders;

public class MainProducerSearchDocumentProvider(
    IMainClient mainClient
) : IProducerSearchDocumentProvider
{
    public async Task<Producer?> GetById(
        int producerId,
        CancellationToken cancellationToken = default)
    {
        var response = await mainClient.ProducerNode.GetFullProducer(producerId, cancellationToken);
        if (response is { StatusCode: HttpStatusCode.NotFound }) return null;

        if (!response.Success)
            throw new InvalidOperationException(
                $"Unable to get producer {producerId} from Main service. " +
                $"Status: {response.StatusCode}. " +
                $"Error: {response.Error}");

        var fullProducer = response.ValueOrThrow;

        return new Producer
        {
            Id = fullProducer.Producer.Id,
            Name = fullProducer.Producer.Name,
            Description = fullProducer.Producer.Description,
            Aliases = fullProducer.Aliases.Select(MapAlias).ToList()
        };
    }

    private static ProducerAlias MapAlias(InternalProducerAlias alias)
    {
        return new ProducerAlias
        {
            Alias = alias.Alias
        };
    }
}