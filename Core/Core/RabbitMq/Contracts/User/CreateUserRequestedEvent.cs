namespace Core.RabbitMq.Contracts.User;

public record CreateUserRequestedEvent(string Name, string Surname, string Email, string UserName, bool IsSupplier);