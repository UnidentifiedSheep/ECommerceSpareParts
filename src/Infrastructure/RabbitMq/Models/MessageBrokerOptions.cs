namespace RabbitMq.Models;

public record MessageBrokerOptions
{
    public string Host { get; init; } = null!;
    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
}