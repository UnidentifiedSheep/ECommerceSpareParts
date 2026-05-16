using System.ComponentModel.DataAnnotations;

namespace RabbitMq;

public class MessageBrokerOptions
{
    public const string SectionName = "MessageBroker";
    
    [Required]
    public required string Url { get; init; }
    
    [Required]
    public required string Username { get; init; }
    
    [Required]
    public required string Password { get; init; }
}