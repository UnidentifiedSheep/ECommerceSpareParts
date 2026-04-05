using System.ComponentModel.DataAnnotations;

namespace RabbitMq.Models;

public record MessageBrokerOptions
{
    public const string SectionName = "MessageBroker";
 
    [Required]
    public required string Host { get; init; }
    
    [Required]
    public required string Username { get; init; }
    
    [Required]
    public required string Password { get; init; }
}