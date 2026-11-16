using Application.Common.Interfaces.Repositories;
using Main.Entities.Product.Producer;

namespace Main.Modules.Product.Application.Interfaces.Persistence;

public interface IProducerRepository : IRepository<Producer, int>
{
	Task<bool> ProducerHasAnyArticle(int producerId, CancellationToken cancellationToken = default);

	Task AddSupplierMappingsOnConflictDoNothingAsync(
		IEnumerable<ProducerSupplierMapping> mappings,
		CancellationToken cancellationToken = default);
}
