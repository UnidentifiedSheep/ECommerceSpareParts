using Application.Common.Interfaces.Repositories;
using Main.Entities.Producer;

namespace Main.Application.Interfaces.Repositories;

public interface IProducerRepository : IRepository<Producer, int>
{
    Task<bool> ProducerHasAnyArticle(int producerId, CancellationToken cancellationToken = default);
}