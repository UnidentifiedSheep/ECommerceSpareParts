using Internal.Integration.Core.Models;
using Internal.Integration.Core.Models.Main;
using Internal.Integration.Core.Models.Main.Producer;

namespace Internal.Integration.Core.Interfaces.Main;

public interface IProducerNode
{
    Task<InternalResponse<InternalFullProducer>> GetFullProducer(int producerId, CancellationToken cancellationToken = default);
}