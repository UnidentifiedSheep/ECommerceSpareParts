using Main.Application.Interfaces.Repositories;
using Main.Entities.Product;
using Main.Persistence.Context;

namespace Main.Persistence.Repositories;

public class ProductWeightRepository(DContext context) : RepositoryBase<ProductWeight, int>(context), IProductWeightRepository
{
    
}