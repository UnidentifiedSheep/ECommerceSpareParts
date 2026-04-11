namespace Domain.Interfaces;

public interface IEntity<out TKey> :  IEntity
{
    new TKey GetId();
}

public interface IEntity
{
    object GetId();
}