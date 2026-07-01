using Integrations.Common;
using Internal.Integration.Core.Models.Main.Producer;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IProducerNode
{
    Task<Response<InternalFullProducer>> GetFullProducer(
        int producerId,
        CancellationToken cancellationToken = default);
}