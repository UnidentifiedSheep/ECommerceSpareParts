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
    public async Task<Dictionary<int, Producer?>> GetByIds(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        var idsList = ids.Distinct().ToList();
        var response = await mainClient.ProducerNode.GetFullProducer(idsList, cancellationToken);
        
        if (!response.Success)
            throw new InvalidOperationException(
                $"Unable to get producers from Main service. " +
                $"Status: {response.StatusCode}. " +
                $"Error: {response.Error}");

        var dict = response.ValueOrThrow
            .ToDictionary(x => x.Id);

        return idsList.ToDictionary(
            x => x, 
            x => dict.TryGetValue(x, out var producer) 
                ? MapProducer(producer) 
                : null);
    }
    
    private static Producer MapProducer(InternalFullProducer producer)
    {
        return new Producer
        {
            Id = producer.Id,
            Name = producer.Name,
            Description = producer.Description,
            Aliases = producer.Aliases.Select(x => new ProducerAlias { Alias = x }).ToList()
        };
    }
}