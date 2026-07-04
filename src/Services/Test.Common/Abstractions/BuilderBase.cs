using Bogus;
using Tests.Interfaces;

namespace Tests.Abstractions;

public abstract class BuilderBase<T>(Faker faker)
    : IBuilder<T>
{
    protected Faker Faker => faker;

    public abstract T Build();

    public virtual IReadOnlyCollection<T> BuildMany(int count)
    {
        return Enumerable.Range(0, count).Select(_ => Build()).ToList();
    }
}