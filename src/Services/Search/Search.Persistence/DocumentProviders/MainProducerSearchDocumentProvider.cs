using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Models.Main;
using Search.Application.Interfaces.Producer;
using Search.Entities;

namespace Search.Persistence.DocumentProviders;

public class MainProducerSearchDocumentProvider(
    IMainClient mainClient) : IProducerSearchDocumentProvider
{
    public async Task<Producer?> GetById(
        int producerId,
        CancellationToken cancellationToken = default)
    {
        var fullProducer = await mainClient.GetFullProducer(producerId, cancellationToken);
        if (fullProducer == null)
        {
            return null;
        }

        return new Producer
        {
            Id = fullProducer.Producer.Id,
            Name = fullProducer.Producer.Name,
            Description = fullProducer.Producer.Description,
            OtherNames = fullProducer.OtherNames.Select(MapOtherName).ToList()
        };
    }

    private static ProducerOtherName MapOtherName(InternalProducerOtherName otherName)
    {
        return new ProducerOtherName
        {
            OtherName = otherName.OtherName,
            WhereUsed = otherName.WhereUsed
        };
    }
}
