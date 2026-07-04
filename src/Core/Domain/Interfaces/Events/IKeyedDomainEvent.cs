namespace Domain.Interfaces.Events;

public interface IKeyedDomainEvent : IDomainEvent
{
    string GetKey();
}