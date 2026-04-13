using Main.Application.Interfaces.Repositories;
using Main.Entities.Product;
using Main.Persistence.Context;
using Persistence;

namespace Main.Persistence.Repositories;

public class ProductWeightRepository(DContext context) : RepositoryBase<DContext, ProductWeight, int>(context), 
    IProductWeightRepository
{
    
}