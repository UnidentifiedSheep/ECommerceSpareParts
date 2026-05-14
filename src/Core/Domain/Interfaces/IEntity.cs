
namespace Domain.Interfaces;

public interface IEntity<TKey> : IEntity
{
    new TKey GetId();
}

public interface IEntity
{
    object GetId();
}