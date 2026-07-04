using Bogus;

namespace Tests.Interfaces;

public interface IFakerContext<TEntity> where TEntity : class
{
    Faker<TEntity> Faker { get; }
    Task InitializeAsync();
}