using Main.Application.Interfaces.Repositories;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Persistence.Context;

public class ReadDContext(DContext context) : IReadDContext
{
    public IQueryable<Product> Products => context.Products.AsQueryable().AsNoTracking();
}