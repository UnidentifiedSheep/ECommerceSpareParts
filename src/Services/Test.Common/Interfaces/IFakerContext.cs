using Bogus;

namespace Test.Common.Interfaces;

public interface IFakerContext<TEntity> where TEntity : class
{
    Faker<TEntity> Faker { get; }
    Task InitializeAsync();
}