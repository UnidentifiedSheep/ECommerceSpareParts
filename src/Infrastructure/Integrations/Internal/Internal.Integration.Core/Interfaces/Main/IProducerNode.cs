using Internal.Integration.Core.Models.Main;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IProducerNode
{
    Task<InternalFullProducer?> GetFullProducer(int producerId, CancellationToken cancellationToken = default);
}