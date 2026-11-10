namespace Domain.Interfaces.Events;

/// <summary>Handled before SQL commands are executed</summary>
public interface IBeforeSaveDomainEvent : IDomainEvent;
