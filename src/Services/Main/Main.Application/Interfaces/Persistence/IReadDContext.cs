using Main.Entities.Product;

namespace Main.Application.Interfaces.Repositories;

public interface IReadDContext
{
    IQueryable<Product> Products { get; }
}