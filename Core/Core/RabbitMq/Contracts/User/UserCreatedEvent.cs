namespace Core.RabbitMq.Contracts.User;

public record UserCreatedEvent(string UserId, string NameSurname, bool IsSupplier, string? Email, string? Description = null);