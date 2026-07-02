using Integrations.Common;
using Internal.Integration.Core.Models.Main.Producer;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IProducerNode
{
    Task<Response<IReadOnlyList<InternalFullProducer>>> GetFullProducer(
        IEnumerable<int> producerIds,
        CancellationToken cancellationToken = default);
}