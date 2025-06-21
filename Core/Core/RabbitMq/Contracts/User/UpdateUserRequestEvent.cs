namespace Core.RabbitMq.Contracts.User;

public record UpdateUserRequestEvent(string? NewName, string? NewSurname, string? NewDescription);